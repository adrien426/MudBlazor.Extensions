﻿using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor.Extensions.Attribute;
using MudBlazor.Extensions.Core;
using Nextended.Core.Extensions;

namespace MudBlazor.Extensions.Components;

/// <summary>
/// A Splitter Component
/// </summary>
public partial class MudExSplitter : IJsMudExComponent<MudExSplitter>
{
    private bool _initialized;
    private string[] _preInitParameters;
    private readonly string _dataId = Guid.NewGuid().ToFormattedId();
    private const string ClassName = "mud-ex-splitter";
    private RenderFragment Inherited() => builder =>
    {
        base.BuildRenderTree(builder);
    };

    /// <inheritdoc/>
    public IJSObjectReference JsReference { get; set; }
    /// <inheritdoc/>
    public IJSObjectReference ModuleReference { get; set; }
    /// <inheritdoc/>
    public ElementReference ElementReference { get; set; }
    
    /// <summary>
    /// Indicates whether the Splitter is currently dragging
    /// </summary>
    public bool IsDragging { get; private set; } = false;
    
    /// <summary>
    /// Callback when dragging starts or ends
    /// </summary>
    [Parameter] public EventCallback<bool> IsDraggingChanged { get; set; }

    /// <summary>
    /// Manually set element for right or down
    /// </summary>
    [Parameter] 
    public ElementReference NextElement { get; set; }

    /// <summary>
    /// Manually set element for left or up
    /// </summary>
    [Parameter]
    public ElementReference PrevElement { get; set; }

    /// <summary>
    /// Indicates whether to update sizes in percentage
    /// </summary>
    [Parameter, SafeCategory("Behavior")] 
    public bool UpdateSizesInPercentage { get; set; } = false;
    /// <summary>
    /// Indicates whether the Splitter is reversed
    /// </summary>
    [Parameter, SafeCategory("Behavior")] 
    public bool Reverse { get; set; } = false;

    private IJsMudExComponent<MudExSplitter> AsJsComponent => this;

    /// <inheritdoc/>
    protected override Task OnInitializedAsync()
    {
        // ReSharper disable once ConstantNullCoalescingCondition
        (UserAttributes ??= new Dictionary<string,object>()).AddOrUpdate("data-id", _dataId);
        if (!IsOverwritten(nameof(Color)))
            Color = MudExColor.Primary;
        if (!IsOverwritten(nameof(Size)))
            Size = 5;
        _initialized = true;
        return base.OnInitializedAsync();
    }

    /// <inheritdoc />
    public override Task SetParametersAsync(ParameterView parameters)
    {
        if (!_initialized)
            _preInitParameters = parameters.ToDictionary().Select(x => x.Key).ToArray();
        return base.SetParametersAsync(parameters);
    }

    private bool IsOverwritten(string paramName) => _preInitParameters?.Contains(paramName) == true;


    /// <inheritdoc/>
    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        Class = $"{ClassName} {Class}";
        if (AsJsComponent.JsReference != null)
            await AsJsComponent.JsReference.InvokeVoidAsync("initialize", Options());
    }

    /// <inheritdoc/>
    public virtual object[] GetJsArguments() => new[] { AsJsComponent.ElementReference, AsJsComponent.CreateDotNetObjectReference(), Options() };

    /// <summary>
    /// Returns current size information for next and preview element
    /// </summary>
    /// <returns></returns>
    public async Task<SizeResponse> GetElementSizes()
    {
        return AsJsComponent.JsReference != null
            ? await AsJsComponent.JsReference.InvokeAsync<SizeResponse>("getSize")
            : null;
    }

    /// <summary>
    /// Resets to initial
    /// </summary>
    public async Task Reset()
    {
        if (AsJsComponent.JsReference != null)
            await AsJsComponent.JsReference.InvokeVoidAsync("reset");            
    }

    [JSInvokable]
    public void OnDragStart()
    {
        Console.WriteLine("DragStart");
        IsDragging = true;
        IsDraggingChanged.InvokeAsync(IsDragging);
    }

    [JSInvokable]
    public void OnDragEnd()
    {
        IsDragging = false;
        IsDraggingChanged.InvokeAsync(IsDragging);
    }

    /// <summary>
    /// Restores the state to last sizes
    /// </summary>    
    public async Task Restore()
    {
        if (AsJsComponent.JsReference != null)
            await AsJsComponent.JsReference.InvokeVoidAsync("restore");
    }

    /// <summary>
    /// Returns options for setting up the Splitter.
    /// </summary>
    private object Options()
    {
        return new
        {
            Id = _dataId,
            Reverse,
            Style = GetStyle(),
            Percentage = UpdateSizesInPercentage,
            VerticalSplit = !Vertical, // Splitter is vertical so container is horizontal,
            PrevElement = !string.IsNullOrEmpty(PrevElement.Id) ? PrevElement as object : null,
            NextElement = !string.IsNullOrEmpty(NextElement.Id) ? NextElement as object : null,
        };
    }

    /// <inheritdoc/>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (firstRender)
            await AsJsComponent.ImportModuleAndCreateJsAsync();
    }

    /// <inheritdoc/>
    public ValueTask DisposeAsync()
    {
        return AsJsComponent.DisposeModulesAsync();
    }

}