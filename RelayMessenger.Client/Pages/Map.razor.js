
export async function initMap(apiKey, mapId, elementReference) {
    debugger;
    const loader = globalThis.relayMessenger.googleMaps.getLoader(apiKey);
    const mapOptions = {
        center: {
            lat: 0,
            lng: 0,
        },
        zoom: 4,
        mapId: mapId
    };

    await loader
        .importLibrary('maps')
        .then(async ({Map}) => {
            const map = new Map(elementReference, mapOptions);
            const {AdvancedMarkerElement} = await loader.importLibrary('marker');
            new AdvancedMarkerElement({map, position: mapOptions.center});
        })
        .catch((e) => {
            // do something
        });
}
