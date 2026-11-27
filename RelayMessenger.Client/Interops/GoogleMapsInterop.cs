using System.Runtime.Versioning;
using Microsoft.JSInterop;
using RelayMessenger.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RelayMessenger.Client.Interops;

public class GoogleMapsInterop
{
    private IJSObjectReference? _mapModule;
    public bool IsInitialized => _mapModule != null;

    public async Task Initialize(IJSObjectReference mapModule)
    {
        _mapModule = mapModule;
    }

    public async Task SyncMarkers(List<NodeStateModel> nodes)
    {
        if (_mapModule == null) return;
        await _mapModule.InvokeVoidAsync("syncMarkers", nodes);
    }

    public async Task SetCenter(double lat, double lng)
    {
        if (_mapModule == null) return;
        await _mapModule.InvokeVoidAsync("setCenter", lat, lng);
    }

    public async Task SetZoom(double zoom)
    {
        if (_mapModule == null) return;
        await _mapModule.InvokeVoidAsync("setZoom", zoom);
    }

    public async Task WatchPosition()
    {
        if (_mapModule == null) return;
        await _mapModule.InvokeVoidAsync("watchPosition");
    }

    public async Task StopPositionWatch()
    {
        if (_mapModule == null) return;
        await _mapModule.InvokeVoidAsync("watchPositionStop");
    }

    public async Task UpdateMyInfoMarker(string message, double lat, double lng)
    {
        if (_mapModule == null) return;
        await _mapModule.InvokeVoidAsync("UpdateMyInfoMarker", message, lat, lng);
    }

    public async Task UpdateCurrentPosition(double lat, double lng)
    {
        if (_mapModule == null) return;
        await _mapModule.InvokeVoidAsync("UpdateCurrentPosition", lat, lng);
    }

    public async Task RemoveInfoMarker(uint id)
    {
        if (_mapModule == null) return;
        await _mapModule.InvokeVoidAsync("RemoveInfoMarker", id);
    }
}
