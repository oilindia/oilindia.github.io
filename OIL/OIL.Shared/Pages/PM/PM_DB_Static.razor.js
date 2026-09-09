// PMDashboardStatic.razor.js

export function initPmDashboardChartsStatic(data) {

    // Helper to prevent memory leaks by destroying previous chart instances
    const destroyChart = (id) => {
        const existing = Chart.getChart(id);
        if (existing) {
            existing.destroy();
        }
    };

    try {
        // PM MAN DAYS TREND (Weekly)
        destroyChart("pmManDaysChart");
        new Chart(document.getElementById("pmManDaysChart"), {
            type: 'bar',
            data: {
                labels: data.weeklyLabels,
                datasets: [{
                    label: 'PM Man Days',
                    data: data.weeklyPmManDays,
                    backgroundColor: '#2563eb',
                    borderRadius: 8
                }]
            },
            options: { responsive: true, maintainAspectRatio: false }
        });

        // TOP INSTALLATIONS
        destroyChart("topInstallationsChart");
        new Chart(document.getElementById("topInstallationsChart"), {
            type: 'bar',
            data: {
                labels: data.topLocationLabels,
                datasets: [{
                    label: 'PM Jobs',
                    data: data.topLocationCounts,
                    backgroundColor: '#16a34a',
                    borderRadius: 8
                }]
            },
            options: {
                indexAxis: 'y',
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    y: { beginAtZero: true, ticks: { autoSkip: false } },
                    x: { beginAtZero: true }
                },
                plugins: { legend: { display: false } }
            }
        });

        // PM SHARE (Doughnut)
        destroyChart("pmShareChart");
        new Chart(document.getElementById("pmShareChart"), {
            type: 'doughnut',
            data: {
                labels: ['Completed PM Jobs', 'Completed Non-PM Jobs'],
                datasets: [{
                    data: data.pmVsNonPm,
                    backgroundColor: ['#2563eb', '#dc2626']
                }]
            },
            options: { responsive: true, maintainAspectRatio: false }
        });

        // FIELD DISTRIBUTION (Central Removed)
        destroyChart("fieldDistributionChart");
        new Chart(document.getElementById("fieldDistributionChart"), {
            type: 'pie',
            data: {
                labels: ['Western Field', 'Eastern Field'],
                datasets: [{
                    data: data.fieldDistribution,
                    backgroundColor: ['#22c55e', '#f97316'] // Green for WF, Orange for EF
                }]
            },
            options: { responsive: true, maintainAspectRatio: false }
        });

        // MAINTENANCE OVERVIEW (Weekly stacked)
        destroyChart("maintenanceOverviewChart");
        new Chart(document.getElementById("maintenanceOverviewChart"), {
            type: 'bar',
            data: {
                labels: data.weeklyLabels,
                datasets: [
                    {
                        label: 'PM Jobs',
                        data: data.weeklyPmCounts,
                        backgroundColor: '#16a34a',
                        stack: 'jobs'
                    },
                    {
                        label: 'Non-PM Jobs',
                        data: data.weeklyNonPmCounts,
                        backgroundColor: '#dc2626',
                        stack: 'jobs'
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    x: { stacked: true },
                    y: { stacked: true, beginAtZero: true }
                }
            }
        });

    } catch (error) {
        console.error("Error drawing PM Dashboard charts:", error);
    }
}