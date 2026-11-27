window.mapInterop = {
    map: null,
    markers: {},

    initMap: function (elementId, lat, lng, zoom = 13) {
        this.map = L.map(elementId).setView([lat, lng], zoom);

        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            maxZoom: 19,
            attribution: '© OpenStreetMap'
        }).addTo(this.map);
    },

    updateMarkers: function (nodes) {
        nodes.forEach(node => {
            if (this.markers[node.NodeId]) {
                this.markers[node.NodeId].setLatLng([node.Latitude, node.Longitude]);
            } else {
                this.markers[node.NodeId] = L.marker([node.Latitude, node.Longitude])
                    .addTo(this.map)
                    .bindPopup(node.Label);
            }
        });
    },

    removeMarkers: function () {
        Object.values(this.markers).forEach(marker => {
            this.map.removeLayer(marker);
        });
        this.markers = {};
    }
};
