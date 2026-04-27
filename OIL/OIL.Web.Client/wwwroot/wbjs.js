window.getElementDimensions = (id) => {
    const el = document.getElementById(id);
    if (!el) return { width: 0, height: 0 };
    return { width: el.offsetWidth, height: el.offsetHeight };
};

window.scrollToMarker = (containerId, xPercent, yPercent) => {
    const container = document.getElementById(containerId).parentElement;
    const map = document.getElementById(containerId);

    if (container && map) {
        const x = (map.clientWidth * (xPercent / 100)) - (container.clientWidth / 2);
        const y = (map.clientHeight * (yPercent / 100)) - (container.clientHeight / 2);

        container.scrollTo({ top: y, left: x, behavior: 'smooth' });
    }
};


window.setupCharts = () => {
    // Bar Chart
    const barCtx = document.getElementById('barChart').getContext('2d');
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

    // Donut Chart
    const donutCtx = document.getElementById('donutChart').getContext('2d');
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
};

window.setupReductionCharts = () => {
    // 1. Horizontal Reduction Chart (Left)
    const ctxReduction = document.getElementById('reductionChart').getContext('2d');
    new Chart(ctxReduction, {
        type: 'bar',
        data: {
            labels: ['Maintenance', 'Employee OT', 'Transport', 'Overall'],
            datasets: [{
                axis: 'y', // This makes it horizontal
                data: [17.0, 36.9, 4.0, 16.8], // The specific % values from previous data
                backgroundColor: [
                    '#d32f2f', // Red (Maintenance)
                    '#20548a', // Dark Blue (Employee OT)
                    '#b0892d', // Gold (Transport)
                    '#1a7f4e'  // Green (Overall)
                ],
                borderWidth: 0,
                borderRadius: 4
            }]
        },
        options: {
            indexAxis: 'y', // Vital for horizontal layout
            responsive: true,
            maintainAspectRatio: false,
            plugins: { legend: { display: false } }, // Hide legend
            scales: {
                x: {
                    title: { display: true, text: '% Reduction', color: '#546e7a' },
                    suggestedMax: 40, // Match the image scale
                    ticks: { color: '#78909c' }
                },
                y: {
                    ticks: { color: '#78909c' },
                    grid: { display: false } // Cleaner look
                }
            }
        }
    });

    // 2. Overtime Hours Chart (Right)
    const ctxOT = document.getElementById('otHoursChart').getContext('2d');
    new Chart(ctxOT, {
        type: 'bar',
        data: {
            labels: ['Q1 & Q2', 'Q3 & Q4'],
            datasets: [{
                data: [4780, 4400], // Example data based on the visual height in the image
                backgroundColor: [
                    '#d1e4f6', // Muted Blue (Before)
                    '#d32f2f'  // Red (After)
                ],
                borderWidth: 0,
                borderRadius: 4,
                barThickness: 60 // Controls the visual width of the bars
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: { legend: { display: false } },
            scales: {
                y: {
                    title: { display: true, text: 'Hours', color: '#546e7a' },
                    ticks: { color: '#78909c' }
                },
                x: {
                    ticks: { color: '#78909c' },
                    grid: { display: false }
                }
            }
        }
    });
};


window.setupSavingsMixChart = () => {
    const ctxSavings = document.getElementById('savingsMixChart').getContext('2d');
    new Chart(ctxSavings, {
        type: 'doughnut',
        data: {
            labels: ['PM & Spares', 'Transport', 'Technology', 'OT Control'],
            datasets: [{
                data: [60, 2, 30, 8], // Percentages based on visual share in image
                backgroundColor: [
                    '#d32f2f', // Red
                    '#b0892d', // Gold
                    '#1a7f4e', // Green
                    '#20548a'  // Dark Blue
                ],
                hoverOffset: 4,
                borderWidth: 2,
                borderColor: '#ffffff'
            }]
        },
        options: {
            cutout: '60%', // Creates the donut hole
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: {
                        usePointStyle: true,
                        padding: 20,
                        font: { size: 12, weight: 'bold' }
                    }
                }
            }
        }
    });
};





window.scrollChatBottom = function () {
    let el = document.getElementById("chat-container");
    if (el) {
        el.scrollTop = el.scrollHeight;
    }
};