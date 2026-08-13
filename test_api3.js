async function run() {
    try {
        const r2 = await fetch("https://acikveriapi.gaziantep.bel.tr/api/Itfaiye/Ihbarlar");
        const json = await r2.json();
        const items = json.data.data;
        if (Array.isArray(items)) {
            console.log("Ihbarlar data length:", items.length);
            console.log("First item:", JSON.stringify(items[0], null, 2));
        }
    } catch (e) {
        console.log("Error:", e);
    }
}
run();
