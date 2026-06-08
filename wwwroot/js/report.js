(function () {
    const el = document.currentScript;

    const labels    = JSON.parse(el.dataset.labels    || '[]');
    const revData   = JSON.parse(el.dataset.rev       || '[]');
    const cntData   = JSON.parse(el.dataset.cnt       || '[]');
    const pieLabels = JSON.parse(el.dataset.pieLabels || '[]');
    const pieData   = JSON.parse(el.dataset.pie       || '[]');

    // ── Bar chart: Doanh thu 6 tháng 
    const barCanvas = document.getElementById('barChart');
    if (barCanvas) {
        new Chart(barCanvas, {
            data: {
                labels: labels,
                datasets: [
                    {
                        type: 'bar',
                        label: 'Doanh thu (triệu)',
                        data: revData,
                        backgroundColor: '#2563eb',
                        borderRadius: 8,
                        yAxisID: 'y',
                    },
                    {
                        type: 'bar',
                        label: 'Số đặt phòng',
                        data: cntData,
                        backgroundColor: '#16a34a',
                        borderRadius: 8,
                        yAxisID: 'y2',
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: { font: { size: 12 } }
                    }
                },
                scales: {
                    y: {
                        position: 'left',
                        grid: { color: '#e2e8f0' },
                        ticks: { color: '#64748b' }
                    },
                    y2: {
                        position: 'right',
                        grid: { drawOnChartArea: false },
                        ticks: {
                            color: '#64748b',
                            // fix số phòng
                            stepSize: 1,
                            callback: function (value) {
                                return Number.isInteger(value) ? value : null;
                            }
                        }
                    },
                    x: {
                        grid: { display: false },
                        ticks: { color: '#64748b' }
                    }
                }
            }
        });
    }

    // ── Pie chart: Phân bổ loại phòng
    const pieCanvas = document.getElementById('pieChart');
    if (pieCanvas) {
        new Chart(pieCanvas, {
            type: 'pie',
            data: {
                labels: pieLabels,
                datasets: [{
                    data: pieData,
                    backgroundColor: ['#2563eb', '#16a34a', '#f59e0b'],
                    borderWidth: 0,
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: { font: { size: 12 }, padding: 16 }
                    }
                }
            }
        });
    }
})();