
// In your wwwroot/index.html or a separate JS file
window.clipboardInterop = {
    setupPasteListener: function (elementId, dotNetHelper) {
        const element = document.getElementById(elementId);
        element.addEventListener('paste', async (e) => {
            const items = e.clipboardData.items;
            for (let i = 0; i < items.length; i++) {
                if (items[i].type.indexOf('image') !== -1) {
                    e.preventDefault(); // Stop the default local path paste
                    const blob = items[i].getAsFile();
                    const reader = new FileReader();

                    reader.onload = function (event) {
                        const base64String = event.target.result;
                        // Send the base64 string back to Blazor
                        dotNetHelper.invokeMethodAsync('HandlePastedImage', base64String);
                    };
                    reader.readAsDataURL(blob);
                }
            }
        });
    }
};


window.downloadFileFromStreamTemplate = async (fileName, contentStreamReference) => {
    const arrayBuffer = await contentStreamReference.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    const url = URL.createObjectURL(blob);
    const anchorElement = document.createElement('a');
    anchorElement.href = url;
    anchorElement.download = fileName ?? '';
    anchorElement.click();
    anchorElement.remove();
    URL.revokeObjectURL(url);
};



window.downloadFileFromStreamStore = async (fileName, contentStream) => {
    const arrayBuffer = await contentStream.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    const url = URL.createObjectURL(blob);
    const anchorElement = document.createElement('a');
    anchorElement.href = url;
    anchorElement.download = fileName ?? '';
    anchorElement.click();
    anchorElement.remove();
    URL.revokeObjectURL(url);
}



//------- Download Excel Logic ---
window.downloadFileFromStream = async (fileName, base64String) => {
    const link = document.createElement('a');
    link.download = fileName;
    link.href = 'data:application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;base64,' + base64String;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}


//------- Download Excel Logic ---

window.downloadFile = (fileName, base64Data) => {

    const link = document.createElement('a');

    link.download = fileName;

    link.href =
        "data:application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;base64,"
        + base64Data;

    document.body.appendChild(link);

    link.click();

    document.body.removeChild(link);
};






//------- Print Logic ---

window.blazorPrint = () => {
    setTimeout(() => {
        window.print();
    }, 500);
};






// --- Schedule & Grid Logic ---
window.scrollToToday = () => {
    const today = new Date().getDate();

    // 85px per cell. Subtracting 2 to keep 'today' slightly centered.
    // Note: If you have sticky columns on the left, you may need to add their 
    // combined width (e.g., + 200) to this scrollAmount for exact precision.
    const scrollAmount = (today - 2) * 85;

    const grids = ['staff-grid', 'driver-grid'];

    grids.forEach(id => {
        // Blazor MudTables sometimes render an inner '.mud-table-container' 
        // that handles the actual overflow. If scrolling the wrapper fails, 
        // target the inner container.
        const wrapper = document.getElementById(id);
        if (wrapper) {
            // Check if MudBlazor created an inner scrollable div
            const innerScroll = wrapper.querySelector('.mud-table-container');
            const target = innerScroll || wrapper;

            target.scrollTo({
                left: scrollAmount,
                behavior: 'smooth'
            });
        }
    });
};

window.getElementDimensions = (el) => {
    if (!el) return { width: 0, height: 0 };
    return { width: el.offsetWidth, height: el.offsetHeight };
};

// --- Digital Twin & Mapping Logic ---
window.scrollToMarker = (containerId, xPercent, yPercent) => {
    const map = document.getElementById(containerId);
    const container = map ? map.parentElement : null;
    if (container && map) {
        const x = (map.clientWidth * (xPercent / 100)) - (container.clientWidth / 2);
        const y = (map.clientHeight * (yPercent / 100)) - (container.clientHeight / 2);
        container.scrollTo({ top: y, left: x, behavior: 'smooth' });
    }
};

window.scrollChatBottom = function () {
    let el = document.getElementById("chat-container");
    if (el) { el.scrollTop = el.scrollHeight; }
};

// --- Maintenance Dashboard Charts ---
window.initAllMaintenanceCharts = () => {
    const labels = ['2023-2024', '2024-2025', '2025-2026'];
    const pmManDays = [72, 152, 258];
    const nonPmJobs = [2644, 2798, 2458];
    const pmPercent = [2.6, 5.1, 10];
    const growthPercent = [53, 146];
    const fieldLabels = ['OCS/EPS/FGGS', 'GCS', 'WI'];
    const fieldValues = [39, 16, 7];
    const overtimeValues = [9000, 7700];

    const chartConfig = (type, labels, datasets, options = {}) => ({
        type,
        data: { labels, datasets },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: { legend: { position: 'bottom' } },
            ...options
        }
    });

    // 1. PM Trend
    new Chart(document.getElementById('pmTrend'), chartConfig('line', labels, [{
        label: 'PM Man Days',
        data: pmManDays,
        borderColor: '#0077b6',
        backgroundColor: 'rgba(0,119,182,0.1)',
        fill: true,
        tension: 0.3
    }]));

    // 2. PM Installs
    new Chart(document.getElementById('pmInstallBar'), chartConfig('bar', labels, [{
        label: 'PM Installs',
        data: [32, 49, 79],
        backgroundColor: '#5b8db8',
        borderRadius: 6
    }]));

    // 3. PM Efficiency %
    new Chart(document.getElementById('pmPercent'), chartConfig('line', labels, [{
        label: 'PM %',
        data: pmPercent,
        borderColor: '#f4a015',
        pointBackgroundColor: '#f4a015',
        tension: 0.3
    }]));

    // 4. YoY Growth
    new Chart(document.getElementById('growthChart'), chartConfig('bar', ['2024-25', '2025-26'], [{
        label: 'Growth %',
        data: growthPercent,
        backgroundColor: '#1db954',
        borderRadius: 6
    }]));

    // 5. Dual Axis Combo Chart
    new Chart(document.getElementById('maintenanceTrendCombo'), {
        type: 'line',
        data: {
            labels,
            datasets: [
                { label: 'PM Jobs', data: pmManDays, borderColor: '#0077b6', yAxisID: 'y' },
                { label: 'Non-PM', data: nonPmJobs, borderColor: '#f4a015', yAxisID: 'y' },
                { label: 'PM %', data: pmPercent, borderColor: '#1db954', yAxisID: 'y1', borderDash: [5, 5] }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                y: { type: 'linear', position: 'left', title: { display: true, text: 'Jobs' } },
                y1: { type: 'linear', position: 'right', grid: { drawOnChartArea: false }, title: { display: true, text: '%' } }
            }
        }
    });

    // 6. Field Area Distribution
    new Chart(document.getElementById('fieldBar'), chartConfig('bar', fieldLabels, [{
        label: 'PM Activities',
        data: fieldValues,
        backgroundColor: ['#0077b6', '#f4a015', '#1db954']
    }]));

    // 7. CF/WF/EF Doughnut
    new Chart(document.getElementById('cfWfEfPie'), chartConfig('doughnut', ['CF', 'WF', 'EF'], [{
        data: [37, 16, 11],
        backgroundColor: ['#0077b6', '#f4a015', '#94a3b8']
    }], { cutout: '60%' }));

    // 8. Overtime Analytics
    new Chart(document.getElementById('totalOvertimeChart'), chartConfig('bar', ['2024-25', '2025-26'], [{
        label: 'OT Hours',
        data: overtimeValues,
        backgroundColor: '#5b8db8',
        borderRadius: 6
    }], { scales: { y: { min: 7000 } } }));
};

// --- Generic Dashboard Charts ---
window.setupCharts = () => {
    const barCtx = document.getElementById('barChart')?.getContext('2d');
    if (barCtx) {
        new Chart(barCtx, {
            type: 'bar',
            data: {
                labels: ['Maintenance', 'Employee OT', 'Transport'],
                datasets: [
                    { label: 'Q1 & Q2', data: [952, 27.98, 54.75], backgroundColor: '#d1e4f6' },
                    { label: 'Q3 & Q4', data: [790, 17.90, 52.54], backgroundColor: '#d32f2f' }
                ]
            },
            options: { responsive: true, maintainAspectRatio: false }
        });
    }

    const quarterlyCtx = document.getElementById('quarterlyCostChart')?.getContext('2d');
    if (quarterlyCtx) {
        new Chart(quarterlyCtx, {
            type: 'bar',
            data: {
                labels: ['Q1', 'Q2', 'Q3', 'Q4'],
                datasets: [{
                    label: 'Cost (₹ Crores)',
                    data: [4.1, 3.98, 3.7, 4.2],
                    backgroundColor: ['#2e7d32', '#1565c0', '#f9a825', '#d32f2f'],
                    categoryPercentage: 1.0,
                    barPercentage: 1.0
                }]
            },
            options: {
                indexAxis: 'y',
                responsive: true,
                maintainAspectRatio: false,
                plugins: { legend: { display: false } },
                scales: { x: { beginAtZero: true }, y: { grid: { display: false } } }
            }
        });
    }

    const donutCtx = document.getElementById('donutChart')?.getContext('2d');
    if (donutCtx) {
        new Chart(donutCtx, {
            type: 'doughnut',
            data: {
                labels: ['Maintenance ₹162L', 'Transport ₹2.21L', 'OT ₹10.08L'],
                datasets: [{
                    data: [162, 2.21, 10.08],
                    backgroundColor: ['#d32f2f', '#b0892d', '#20548a'],
                    borderWidth: 2
                }]
            },
            options: { cutout: '70%', plugins: { legend: { position: 'bottom' } } }
        });
    }
};

window.setupReductionCharts = () => {
    const ctxReduction = document.getElementById('reductionChart')?.getContext('2d');
    if (ctxReduction) {
        new Chart(ctxReduction, {
            type: 'bar',
            data: {
                labels: ['Maintenance', 'Employee OT', 'Transport', 'Overall'],
                datasets: [{
                    axis: 'y',
                    data: [17.0, 36.9, 4.0, 16.8],
                    backgroundColor: ['#d32f2f', '#20548a', '#b0892d', '#1a7f4e'],
                    borderRadius: 4
                }]
            },
            options: {
                indexAxis: 'y',
                responsive: true,
                maintainAspectRatio: false,
                plugins: { legend: { display: false } },
                scales: { x: { suggestedMax: 40 } }
            }
        });
    }
};

window.setupSavingsMixChart = () => {
    const ctxSavings = document.getElementById('savingsMixChart')?.getContext('2d');
    if (ctxSavings) {
        new Chart(ctxSavings, {
            type: 'doughnut',
            data: {
                labels: ['PM & Spares', 'Transport', 'Technology', 'OT Control'],
                datasets: [{
                    data: [60, 2, 30, 8],
                    backgroundColor: ['#d32f2f', '#b0892d', '#1a7f4e', '#20548a'],
                    borderWidth: 2
                }]
            },
            options: { cutout: '60%', responsive: true, maintainAspectRatio: false }
        });
    }
};
