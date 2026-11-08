import { mount } from "../_content/Silmoon.AspNetCore.Blazor/Components/DataGrid.razor.js";

function cell(value, type) {
    const element = document.createElement("td");
    element.textContent = String(value);
    element.dataset.gridValue = String(value);
    if (type) element.dataset.gridType = type;
    return element;
}

export function mountGrid(host) {
    if (!host?.isConnected) return null;
    const root = host.querySelector("[data-demo-js-grid]");
    const status = host.querySelector("[data-demo-js-grid-value]");
    const controller = mount(root, {
        getKey: item => item.id,
        createRow: item => {
            const row = document.createElement("tr");
            row.append(cell(item.id, "number"), cell(item.name), cell(item.score, "number"));
            return row;
        },
        onSelectionChange: key => status.textContent = key == null ? "已清除 JavaScript 选择。" : `JavaScript 已选择行：${key}`,
        onRowClick: detail => status.textContent = `JavaScript 单击：${detail.item.name}，显示行号 ${detail.rowIndex + 1}`,
        onRowDoubleClick: detail => status.textContent = `JavaScript 双击：${detail.item.name}，显示行号 ${detail.rowIndex + 1}`
    });
    controller.setItems([
        { id: 13, name: "Lambda", score: 84 },
        { id: 11, name: "Iota", score: 91 },
        { id: 14, name: "Mu", score: 77 },
        { id: 12, name: "Kappa", score: 89 }
    ]);
    status.textContent = "JavaScript 数据已加载。";
    return controller;
}
