window.relayMessenger = {
    googleMaps: {
        CreateMapModule: function() {
            const mapElement = document.getElementById('map');
            if (!mapElement) throw new Error("Map element not found");

            const map = new google.maps.Map(mapElement, {
                center: { lat: 0, lng: 0 },
                zoom: 2,
                mapTypeId: 'roadmap'
            });

            const markers = new Map();

            const module = {
                syncMarkers: function(nodes) {
                    // Remove old markers
                    markers.forEach(marker => marker.setMap(null));
                    markers.clear();

                    // Add/Update nodes
                    nodes.forEach(node => {
                        if (node.Id === 0) return; // Broadcast doesn't have a position
                        const marker = new google.maps.Marker({
                            position: { lat: node.Latitude, lng: node.Longitude },
                            map: map,
                            title: node.Name,
                            icon: 'http://maps.google.com/mapfiles/ms/icons/blue-dot.png'
                        });
                        markers.set(node.Id, marker);
                    });
                },

                setCenter: function(lat, lng) {
                    map.setCenter({ lat, lng });
                },

                setZoom: function(zoom) {
                    map.setZoom(zoom);
                },

                watchPosition: function() {
                    // Placeholder for real geolocation watch
                    console.log("Watching position...");
                },

                watchPositionStop: function() {
                    console.log("Stopped watching position.");
                },

                UpdateMyInfoMarker: function(message, lat, lng) {
                    console.log(`UI Update: ${message} at ${lat}, ${lng}`);
                },

                UpdateCurrentPosition: function(lat, lng) {
                    map.panTo({ lat, lng });
                },

                RemoveInfoMarker: function(id) {
                    if (markers.has(id)) {
                        markers.get(id).setMap(null);
                        markers.delete(id);
                    }
                },

                addListenerOnceMapIdle: function(dotNetRef) {
                    google.maps.event.addListenerOnce(map, 'idle', () => {
                        dotNetRef.invokeMethodAsync('OnMapIdle');
                    });
                }
            };

            return module;
        }
    }
};