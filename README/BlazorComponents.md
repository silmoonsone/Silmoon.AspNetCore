# Silmoon.AspNetCore.Blazor 组件指南

本文对应仓库当前版本。`Silmoon.AspNetCore.Blazor` 提供的通用组件使用命名空间 `Silmoon.AspNetCore.Blazor.Components`。

## 安装与宿主配置

```powershell
dotnet add package Silmoon.AspNetCore.Blazor
```

在 `_Imports.razor` 中引入命名空间：

```razor
@using Silmoon.AspNetCore.Blazor.Components
```

需要响应 Blazor 事件的页面必须使用交互式渲染模式，例如：

```razor
@rendermode InteractiveServer
```

标准 Blazor Web App 会自动加载 RCL 的初始化模块和 scoped CSS。宿主应保留应用自身的 `{AssemblyName}.styles.css` 引用。使用 `bi-*` 图标时需要加载 Bootstrap Icons；`DropdownMenu` 还需要 Bootstrap 5 JavaScript bundle（含 Popper）。其他组件不依赖模板项目的 `site.js` 或 Modern Color。

## ContentCard

通用内容容器，标题、品牌、说明、图标和页脚均为可选项。

```razor
<ContentCard Title="用户资料"
             Brand="Account"
             Description="编辑当前用户的公开资料。"
             Icon="bi-person-circle"
             MaxWidth="480px"
             Centered="true"
             HeadingLevel="1">
    <ChildContent>
        <div>卡片主体</div>
    </ChildContent>
    <FooterContent>卡片页脚</FooterContent>
</ContentCard>
```

`Width` 和 `MaxWidth` 默认为 `100%`，`HeadingLevel` 会限制在 1–6。表单专用样式只会应用于显式使用 `card-form`、`card-fields`、`card-actions` 等类的内容。

## TimeInput

`TimeInput` 绑定 `TimeSpan`，使用固定的 `HH:mm` 格式；启用 `AllowEndOfDay` 后可用 `24:00` 表示一天结束。

```razor
<TimeInput @bind-Value="EndTime"
           AllowEndOfDay="true"
           class="form-control"
           aria-label="结束时间" />

@code {
    TimeSpan EndTime { get; set; } = new(23, 30, 0);
}
```

控件在输入完整、有效的时间后更新绑定值。输入范围是 `00:00`–`23:59`，启用 `AllowEndOfDay` 时也接受 `24:00`。

## EnumSelect&lt;TEnum&gt;

从枚举生成选择列表，并读取 `DisplayAttribute` 的名称和说明。

```razor
<EnumSelect TEnum="OrderState"
            @bind-Value="State"
            ShowDescriptions="true"
            aria-label="订单状态" />

@code {
    enum OrderState
    {
        [Display(Name = "待处理", Description = "等待后台任务领取")]
        Pending,
        [Display(Name = "已完成", Description = "任务已经完成")]
        Completed
    }

    OrderState State { get; set; } = OrderState.Pending;
}
```

可通过 `Options` 限制允许选择的枚举成员，通过 `Disabled` 禁用控件。

## TreePicker

从规范化的目录路径集合中生成树形选择器。路径使用 `/目录/子目录/` 格式。

```razor
<TreePicker Folders="Folders"
            @bind-Value="Folder"
            AllowCreate="true" />

@code {
    IReadOnlyCollection<string> Folders { get; } =
        ["/", "/产品/", "/产品/API/", "/归档/"];
    string Folder { get; set; } = "/产品/API/";
}
```

目录树展示当前 `Folders`、绑定路径 `Value` 以及它们的上级目录。切换数据时，一起更新 `Folders` 和 `Value` 即可切换目录列表。`AllowCreate` 允许在选中目录下创建一级临时目录。只有通过控件手动创建的目录会额外保留在当前组件实例中。使用 `Disabled` 可同时禁止选择和创建。

## DateTimeRangePicker

选择日期或日期时间范围，并提供今天、昨天、本周、本月、近 30 天等快捷项。

```razor
<DateTimeRangePicker @bind-Value="Range" IncludeTime="true" />

@code {
    DateTimeRangeValue? Range { get; set; }
}
```

`DateTimeRangeValue` 包含 `StartTime`、`EndTime` 和可选 `Label`。组件按浏览器本地时区显示和输入，应用后返回 UTC `DateTimeOffset`。`Texts` 可替换内置界面文本。

使用 `IncludeTime="true"` 编辑已有范围时，只改一端会保留另一端的原始时刻和精度，即使该时间位于夏令时回拨时重复出现的小时内。直接确认原范围也会保留其 `Label`。使用 `IncludeTime="false"` 时，所选范围覆盖完整的本地日期，从起始日的零点到结束日次日零点前一毫秒。

## DropdownSelect&lt;TValue&gt;

带图标、说明、禁用项和可选导航链接的单选下拉框。

```razor
<DropdownSelect TValue="string"
                Items="EnvironmentOptions"
                @bind-Value="Environment"
                Label="运行环境"
                Placeholder="选择环境"
                IconClass="bi bi-hdd-network" />

@code {
    string Environment { get; set; } = "production";
    IReadOnlyList<DropdownSelectOption<string>> EnvironmentOptions { get; } =
    [
        new() { Value = "development", Text = "开发环境", Description = "本机调试" },
        new() { Value = "production", Text = "生产环境", IconClass = "bi bi-cloud-check" },
        new() { Value = "retired", Text = "已停用", Disabled = true }
    ];
}
```

`AlignEnd` 控制结束端对齐，`OpenUpward` 优先向上展开；组件仍会根据可用视口空间调整位置。含 `Href` 的选项执行导航而不修改绑定值。设置 `EnhanceNavigation="false"` 后，链接会实际输出 `data-enhance-nav="false"`。

`JavaScriptManaged="true"` 用于非 Blazor 状态所有者。此时根元素触发冒泡的 `change` 事件，可通过 `window.DropdownSelect.bind(root)` 读取或设置值。

## DropdownMenu

支持多级子菜单、禁用项、已选项、按钮回调和导航链接。

```razor
<DropdownMenu Items="MenuItems"
              ItemSelected="OnMenuItemSelected"
              Label="打开操作菜单"
              TriggerText="操作"
              IconClass="bi bi-three-dots" />

@code {
    IReadOnlyList<DropdownMenuItem> MenuItems { get; } =
    [
        new() { Key = "refresh", Text = "刷新", IconClass = "bi bi-arrow-clockwise" },
        new()
        {
            Key = "export",
            Text = "导出",
            Children =
            [
                new() { Key = "csv", Text = "CSV" },
                new() { Key = "json", Text = "JSON", Selected = true }
            ]
        },
        new() { Key = "disabled", Text = "不可用", Disabled = true }
    ];

    void OnMenuItemSelected(DropdownMenuItem item) { }
}
```

`MenuWidth` 默认是 `12.5rem`，也可传入 CSS 长度或 `auto`。`AlignEnd`、`OpenUpward` 和 `EnhanceNavigation` 的行为与 `DropdownSelect` 一致。含 `Href` 的末级项目执行导航，不触发 `ItemSelected`。

菜单展开期间，若父组件在同一个 `Items` 列表中删除或替换当前子菜单，控件会返回根菜单，可继续选择其他操作。

## ConfirmDialog

通过 `@ref` 发起可等待的确认操作，不需要回调函数。

```razor
<button type="button" @onclick="DeleteAsync">删除</button>
<ConfirmDialog @ref="Dialog" />

@code {
    ConfirmDialog? Dialog { get; set; }

    async Task DeleteAsync()
    {
        if (Dialog is null) return;
        var confirmed = await Dialog.ConfirmAsync(
            "确认删除当前记录吗？",
            "删除确认",
            "删除",
            "取消",
            danger: true);
        if (!confirmed) return;
        // 执行删除。
    }
}
```

三选项对话框使用 `ChooseAsync`，返回 `ConfirmDialogResult.Confirmed`、`Alternative` 或 `Cancelled`：

```csharp
var result = await Dialog.ChooseAsync("请选择处理方式。", "稍后处理");
```

同一页面一次只显示一个确认对话框；对话框关闭、页面离开或组件释放时，等待任务会安全结束。

## DataGrid&lt;TItem&gt;

Blazor 管理模式由 Razor 模板提供表头和数据行。排序列需要 `data-grid-sort`，用于排序的单元格可通过 `data-grid-value` 和 `data-grid-type="number"` 指定原始值和数字类型。

```razor
<DataGrid TItem="ProductRow"
          Items="Rows"
          RowKey="row => row.Id.ToString()"
          Label="产品表格"
          FilterLabel="筛选产品"
          EmptyFilterText="没有匹配记录"
          SingleSelect="true"
          OnRowClick="OnRowClick"
          OnRowDoubleClick="OnRowDoubleClick"
          OnSelectionChanged="OnSelectionChanged">
    <Header>
        <th data-grid-column="id"><button type="button" data-grid-sort="id">编号</button></th>
        <th data-grid-column="name"><button type="button" data-grid-sort="name">名称</button></th>
        <th data-grid-column="score"><button type="button" data-grid-sort="score">分数</button></th>
    </Header>
    <RowTemplate Context="row">
        <td data-grid-value="@row.Id" data-grid-type="number">@row.Id</td>
        <td>@row.Name</td>
        <td data-grid-value="@row.Score" data-grid-type="number">@row.Score</td>
    </RowTemplate>
</DataGrid>

@code {
    IReadOnlyList<ProductRow> Rows { get; } = [new(1, "Alpha", 96)];

    void OnRowClick(DataGridRowEvent<ProductRow> args) { }
    void OnRowDoubleClick(DataGridRowEvent<ProductRow> args) { }
    void OnSelectionChanged(string? key) { }

    sealed record ProductRow(int Id, string Name, int Score);
}
```

`RowKey` 应返回稳定且唯一的键。`DataGridRowEvent<TItem>` 提供当前数据项、键、列名、可见行号和列号。行内按钮、链接、输入框等交互元素不会触发行点击回调。

在 Blazor Server 中，即使使用 GUID 等较长的行键，也可在默认 SignalR 消息大小限制下排序。宿主沿用上面的 `RowKey`、表头和行模板即可。

JavaScript 管理模式设置 `JavaScriptManaged="true"`，然后导入模块并挂载真实的 `[data-grid]` 根元素：

```javascript
import { mount } from "/_content/Silmoon.AspNetCore.Blazor/Components/DataGrid.razor.js";

const controller = mount(root, {
    getKey: item => item.id,
    createRow: item => {
        const row = document.createElement("tr");
        row.innerHTML = `<td data-grid-value="${item.id}" data-grid-type="number">${item.id}</td><td></td>`;
        row.cells[1].textContent = item.name;
        return row;
    },
    onSelectionChange: key => console.log(key)
});

controller.setItems([{ id: 1, name: "Alpha" }]);
// 页面或宿主释放时调用：controller.dispose();
```

不要把不可信文本拼接进 `innerHTML`；示例仅对数字编号使用模板字符串，名称通过 `textContent` 写入。

## 静态页面和增强导航

Blazor Web App 会自动导入 `Silmoon.AspNetCore.Blazor.lib.module.js`。非 Blazor 的 MVC/Razor Pages 布局可显式加载组件模块标记支持：

```html
<script type="module" src="/_content/Silmoon.AspNetCore.Blazor/js/component-modules.js"></script>
```

组件内部模块只允许从本 RCL 的 `Components/` 目录加载。页面自己的 JavaScript 应由页面单独导入。

在包含 `<base href="/">` 的 Blazor 应用中，纯 `#fragment` 可能相对应用基址解析到首页。页内导航应使用明确的页面路径和 fragment，例如：

```html
<a href="/component-test#component-data-grids" data-enhance-nav="false">数据表格</a>
```

## 迁移与排错

- 升级到 RCL 组件后，应删除宿主项目中同名的本地 Razor、CSS、JS 和模型类，避免类型或静态资源冲突。
- 浏览器仍执行旧脚本时，先清理浏览器缓存和构建输出，再确认 `_content/Silmoon.AspNetCore.Blazor/` 请求返回 200。
- 弹层被裁切时，确认浏览器支持原生 Popover；回退模式无法突破所有祖先裁切上下文。
- `DropdownMenu` 无法打开时，首先确认 Bootstrap 5 bundle 已加载。
- JavaScript 管理的 DataGrid 在增强导航后必须针对新的 DOM 根元素重新 `mount`，并释放旧 controller。

仓库中的完整运行示例位于 `Silmoon.AspNetCore.Test/RazorPages/ComponentTestRp.razor`，启动 `Silmoon.AspNetCore.Test` 后访问 `/component-test`。
