export async function initLuckysheet(containerId, dotNetRef, initialData = null) {
    if (typeof luckysheet === 'undefined') {
        console.error("Luckysheet dependencies are missing.");
        return false;
    }

    const container = document.getElementById(containerId);
    if (!container) {
        console.error(`Luckysheet container with ID '${containerId}' was not found in the DOM.`);
        return false;
    }

    ensureLogoOverlay(container);
    const sheetData = initialData ? null : generateHeaderGridData();

    try {
        luckysheet.create({
            container: containerId,
            title: 'OIL India PM Editor',
            lang: 'en',
            showinfobar: false,
            showtoolbar: false,
            sheetFormulaBar: false,
            showsheetbar: false,
            showstatisticBar: false,
            rowHeaderWidth: 0,
            columnHeaderHeight: 0,
            allowEdit: true,
            data: initialData || [
                {
                    name: "Report",
                    color: "",
                    status: "1",
                    order: "0",
                    data: sheetData,
                    config: {
                        rowlen: {
                            0: 35, 1: 30, 2: 30, 3: 30, 4: 30, 5: 30, 6: 35, 7: 35
                        },
                        // Exact A4 portrait width optimization (Total: ~790px)
                        columnlen: {
                            0: 5,   // Margin / Padding
                            1: 125, // Logo / Section Column
                            2: 75,  // Unit
                            3: 95,  // Instrument
                            4: 95,  // Make & Model
                            5: 80,  // Range
                            6: 80,  // Servicing - Clean
                            7: 95,  // Calibration Certificate No.
                            8: 140  // Remarks
                        },
                        merge: {
                            "0_1": { r: 0, c: 1, rs: 5, cs: 1 },
                            "0_2": { r: 0, c: 2, rs: 1, cs: 7 },
                            "1_2": { r: 1, c: 2, rs: 1, cs: 7 },
                            "2_2": { r: 2, c: 2, rs: 1, cs: 7 },
                            "3_2": { r: 3, c: 2, rs: 1, cs: 7 },
                            "4_2": { r: 4, c: 2, rs: 1, cs: 7 },
                            "5_2": { r: 5, c: 2, rs: 1, cs: 7 },
                            "6_1": { r: 6, c: 1, rs: 1, cs: 5 },
                            "6_6": { r: 6, c: 6, rs: 1, cs: 3 },
                            "7_1": { r: 7, c: 1, rs: 1, cs: 8 }
                        }
                    }
                }
            ],
            hook: {
                cellEditEnd: function (cell, oldvalue, r, c) {
                    autoFitRow(r);
                },
                updated: function () {
                    if (window._autoFitTimer) clearTimeout(window._autoFitTimer);
                    window._autoFitTimer = setTimeout(() => {
                        autoFitAllRows();
                    }, 350);

                    try {
                        if (dotNetRef) dotNetRef.invokeMethodAsync('OnDataChanged');
                    } catch (err) { }
                }
            }
        });

        setTimeout(() => {
            autoFitAllRows();
        }, 600);

        return true;
    } catch (ex) {
        console.error("Failed to initialize Luckysheet instance:", ex);
        return false;
    }
}

export function getExcelData() {
    if (typeof luckysheet !== "undefined") {
        const fileData = luckysheet.getluckysheetfile();
        return JSON.stringify(fileData);
    }
    return null;
}

export function loadExcelData(containerId, jsonString, dotNetRef) {
    if (typeof luckysheet === "undefined" || !jsonString) return false;

    try {
        const parsedData = JSON.parse(jsonString);
        luckysheet.destroy();
        return initLuckysheet(containerId, dotNetRef, parsedData);
    } catch (ex) {
        console.error("Failed to load sheet data:", ex);
        return false;
    }
}

// Fixed print function that resizes canvas to exact full A4 printable width
export function triggerPrint() {
    const container = document.getElementById('excel-container');
    if (!container) {
        window.print();
        return;
    }

    autoFitAllRows();

    const file = luckysheet.flowdata();
    let maxRow = 10;
    if (file) {
        for (let r = 0; r < file.length; r++) {
            if (!file[r]) continue;
            for (let c = 0; c < file[r].length; c++) {
                const cell = file[r][c];
                if (cell && cell.v !== null && cell.v !== undefined && cell.v !== "") {
                    maxRow = Math.max(maxRow, r);
                }
            }
        }
    }

    const cfg = luckysheet.getConfig() || {};
    const rowlen = cfg.rowlen || {};
    let populatedHeight = 0;

    const targetRow = Math.min(file ? file.length : 100, maxRow + 2);
    for (let i = 0; i <= targetRow; i++) {
        populatedHeight += (rowlen[i] !== undefined ? rowlen[i] : 26);
    }

    const originalHeight = container.style.height;
    const originalWidth = container.style.width;

    // Set exact printable width (790px) to stretch canvas across the full page
    container.style.width = '790px';
    container.style.height = (populatedHeight + 20) + 'px';

    try {
        luckysheet.resize();
    } catch (e) { }

    setTimeout(() => {
        window.print();
        setTimeout(() => {
            container.style.height = originalHeight;
            container.style.width = originalWidth;
            try {
                luckysheet.resize();
            } catch (e) { }
        }, 800);
    }, 300);
}

function ensureLogoOverlay(container) {
    const existingLogo = document.getElementById('excel-logo-overlay');
    if (!existingLogo) {
        const logoImg = document.createElement('img');
        logoImg.id = 'excel-logo-overlay';
        logoImg.src = '_content/OIL.Shared/imgs/oil2.png';
        logoImg.style.position = 'absolute';
        logoImg.style.left = '8px';
        logoImg.style.top = '3px';
        logoImg.style.width = '120px';
        logoImg.style.height = '138px';
        logoImg.style.zIndex = '50';
        logoImg.style.pointerEvents = 'none';
        container.appendChild(logoImg);
    }
}

function autoFitRow(r) {
    try {
        const file = luckysheet.flowdata();
        if (!file || !file[r]) return;

        const cfg = luckysheet.getConfig() || {};
        const merges = cfg.merge || {};
        const colWidths = { 0: 5, 1: 125, 2: 75, 3: 95, 4: 95, 5: 80, 6: 80, 7: 95, 8: 140 };

        let maxLines = 1;

        for (let c = 0; c < file[r].length; c++) {
            const cell = file[r][c];
            if (cell) {
                cell.tb = 2;
                if (cell.v !== null && cell.v !== undefined && cell.v !== "") {
                    const text = cell.v.toString();
                    let cellWidth = colWidths[c] || 85;
                    const mergeKey = `${r}_${c}`;

                    if (merges[mergeKey]) {
                        const cs = merges[mergeKey].cs || 1;
                        cellWidth = 0;
                        for (let i = 0; i < cs; i++) {
                            cellWidth += (colWidths[c + i] || 85);
                        }
                    }

                    const maxCharsPerLine = Math.max(8, Math.floor(cellWidth / 6.0));
                    let lines = 0;
                    text.split('\n').forEach(line => {
                        lines += Math.max(1, Math.ceil(line.length / maxCharsPerLine));
                    });
                    if (lines > maxLines) maxLines = lines;
                }
            }
        }

        const defaultHeight = (r < 8) ? 30 : 26;
        const newHeight = maxLines === 1 ? defaultHeight : Math.max(defaultHeight, maxLines * 18 + 6);

        luckysheet.setRowHeight({ [r]: newHeight });
    } catch (e) {
        Console.error("Error auto-fitting row " + r, e);
    }
}

function autoFitAllRows() {
    try {
        const file = luckysheet.flowdata();
        if (!file) return;
        for (let r = 0; r < file.length; r++) {
            autoFitRow(r);
        }
    } catch (e) { }
}

function generateHeaderGridData() {
    let data = Array(100).fill(0).map(() => Array(10).fill({}));

    function setCell(r, c, text, bold = false, align = "center", size = "11") {
        data[r][c] = {
            v: text,
            m: text,
            ct: { fa: "General", t: "s" },
            bl: bold ? 1 : 0,
            ht: align === "center" ? 0 : (align === "right" ? 2 : 1),
            vt: 0,
            fs: size,
            tb: 2
        };
    }

    setCell(0, 2, "ऑइल इंडिया लिमिटेड", true, "center", "14");
    setCell(1, 2, "यंत्रीकरण विभाग", true, "center", "13");
    setCell(2, 2, "INSTRUMENTATION DEPARTMENT", true, "center", "13");
    setCell(3, 2, "दुलियाजान / DULIAJAN", true, "center", "12");
    setCell(4, 2, "OIL INDIA LIMITED", true, "center", "13");
    setCell(5, 2, "(केवल आंतरिक उपयोग हेतु / For Internal Use Only)", true, "center", "11");
    setCell(6, 1, "Ref no: ", true, "left", "11");
    setCell(6, 6, "PM Date: 28/07/2026, 29/07/2026", true, "left", "11");
    setCell(7, 1, "Notification Number: ", true, "left", "11");

    return data;
}