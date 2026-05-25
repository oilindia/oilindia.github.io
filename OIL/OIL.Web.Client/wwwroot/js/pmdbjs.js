window.initPmDashboardCharts = (data) => {

    const destroyChart = (id) => {
        const existing = Chart.getChart(id);

        if (existing) {
            existing.destroy();
        }
    };

    // PM MAN DAYS

    destroyChart("pmManDaysChart");

    new Chart(document.getElementById("pmManDaysChart"), {
        type: 'bar',
        data: {
            labels: data.dailyLabels,
            datasets: [{
                label: 'PM Man Days',
                data: data.dailyPmManDays,
                backgroundColor: '#2563eb',
                borderRadius: 8
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false
        }
    });

    // TOP INSTALLATIONS

    destroyChart("topInstallationsChart");

    new Chart(document.getElementById("topInstallationsChart"), {
        type: 'bar',
        data: {
            labels: data.topLocationLabels, // Ensure this is an array like ["Loc A", "Loc B", ...]
            datasets: [{
                label: 'PM Jobs',
                data: data.topLocationCounts, // Ensure this is an array like [10, 5, ...]
                backgroundColor: '#16a34a',
                borderRadius: 8
            }]
        },
        options: {
            indexAxis: 'y', // This makes it a horizontal bar chart
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        autoSkip: false // Ensures all labels are shown even if crowded
                    }
                },
                x: {
                    beginAtZero: true
                }
            },
            plugins: {
                legend: { display: false }
            }
        }
    });

    // PM SHARE

    destroyChart("pmShareChart");

    new Chart(document.getElementById("pmShareChart"), {
        type: 'doughnut',
        data: {
            labels: ['PM', 'NON PM'],
            datasets: [{
                data: data.pmVsNonPm,
                backgroundColor: [
                    '#2563eb',
                    '#dc2626'
                ]
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false
        }
    });

    // FIELD DISTRIBUTION

    destroyChart("fieldDistributionChart");

    new Chart(document.getElementById("fieldDistributionChart"), {
        type: 'pie',
        data: {
            labels: [
                'Central',
                'Western',
                'Eastern'
            ],
            datasets: [{
                data: data.fieldDistribution,
                backgroundColor: [
                    '#0ea5e9',
                    '#22c55e',
                    '#f97316'
                ]
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false
        }
    });

    // MAINTENANCE OVERVIEW

    destroyChart("maintenanceOverviewChart");

    new Chart(document.getElementById("maintenanceOverviewChart"), {
        type: 'bar',
        data: {
            labels: data.dailyLabels,
            datasets: [
                {
                    label: 'PM',
                    data: data.dailyPmCounts,
                    backgroundColor: '#16a34a',
                    stack: 'jobs'
                },
                {
                    label: 'NON PM',
                    data: data.dailyNonPmCounts,
                    backgroundColor: '#dc2626',
                    stack: 'jobs'
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                x: {
                    stacked: true
                },
                y: {
                    stacked: true,
                    beginAtZero: true
                }
            }
        }
    });

}; 