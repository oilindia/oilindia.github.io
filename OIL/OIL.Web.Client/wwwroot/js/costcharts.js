window.costDashboard = {
    instances: {},

    renderAll: function (payload) {
        // Shared Color Palette to match MudBlazor
        const colors = {
            overtime: '#3b82f6', // Blue
            spares: '#10b981',   // Green
            employee: '#6366f1', // Indigo
            travel: '#f59e0b',   // Amber
            cumulative: '#ec4899', // Pink
            staff: '#8b5cf6'     // Purple
        };

        this.initChart('chartDonut', 'doughnut', {
            labels: payload.costNames,
            datasets: [{
                data: payload.totals,
                backgroundColor: [colors.overtime, colors.spares, colors.employee, colors.travel]
            }]
        }, { responsive: true, maintainAspectRatio: false, plugins: { legend: { position: 'bottom' } } });

        this.initChart('chartDailyStacked', 'bar', {
            labels: payload.labelsDaily,
            datasets: [
                { label: 'Overtime', data: payload.dailyOvertime, backgroundColor: colors.overtime },
                { label: 'Spares', data: payload.dailySpares, backgroundColor: colors.spares },
                { label: 'Employee', data: payload.dailyEmployee, backgroundColor: colors.employee },
                { label: 'Travel', data: payload.dailyTravel, backgroundColor: colors.travel }
            ]
        }, {
            responsive: true, maintainAspectRatio: false,
            scales: { x: { stacked: true }, y: { stacked: true } },
            plugins: { legend: { position: 'bottom' } }
        });

        this.initChart('chartCumulative', 'line', {
            labels: payload.labelsDaily,
            datasets: [{
                label: 'Running Total (₹)',
                data: payload.cumulativeTotal,
                borderColor: colors.cumulative,
                backgroundColor: 'rgba(236, 72, 153, 0.1)',
                fill: true,
                tension: 0.4
            }]
        }, { responsive: true, maintainAspectRatio: false, plugins: { legend: { position: 'bottom' } } });

        this.initChart('chartQuarterly', 'bar', {
            labels: payload.labelsQuarterly,
            datasets: [
                { label: 'Overtime', data: payload.quarterlyOvertime, backgroundColor: colors.overtime },
                { label: 'Spares', data: payload.quarterlySpares, backgroundColor: colors.spares },
                { label: 'Employee', data: payload.quarterlyEmployee, backgroundColor: colors.employee },
                { label: 'Travel', data: payload.quarterlyTravel, backgroundColor: colors.travel }
            ]
        }, { responsive: true, maintainAspectRatio: false, plugins: { legend: { position: 'bottom' } } });

        this.initChart('chartResourceTrend', 'line', {
            labels: payload.labelsDaily,
            datasets: [
                { label: 'OT Hours', data: payload.dailyOtHours, borderColor: colors.overtime, yAxisID: 'y' },
                { label: 'Active Staff', data: payload.dailyPersonnelCount, borderColor: colors.staff, yAxisID: 'y1' }
            ]
        }, {
            responsive: true, maintainAspectRatio: false,
            scales: {
                y: { type: 'linear', display: true, position: 'left', title: { display: true, text: 'Hours' } },
                y1: { type: 'linear', display: true, position: 'right', grid: { drawOnChartArea: false }, title: { display: true, text: 'Personnel' } }
            },
            plugins: { legend: { position: 'bottom' } }
        });

        this.initChart('chartRadar', 'radar', {
            labels: payload.costNames,
            datasets: [{
                label: 'Cost Vector Shape',
                data: payload.totals,
                backgroundColor: 'rgba(99, 102, 241, 0.2)',
                borderColor: colors.employee,
                pointBackgroundColor: colors.employee
            }]
        }, { responsive: true, maintainAspectRatio: false, plugins: { legend: { position: 'bottom' } } });
    },

    initChart: function (canvasId, type, data, options) {
        const ctx = document.getElementById(canvasId);
        if (!ctx) return;
        if (this.instances[canvasId]) {
            this.instances[canvasId].destroy();
        }
        this.instances[canvasId] = new Chart(ctx, { type, data, options });
    }
};