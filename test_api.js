async function run() {
    try {
        console.log("--- YanginNoktalari ---");
        const r1 = await fetch("https://acikveriapi.gaziantep.bel.tr/api/Itfaiye/YanginNoktalari");
        const data1 = await r1.json();
        if (Array.isArray(data1)) {
            console.log("Type: List, Count:", data1.length);
            console.log("First item:", JSON.stringify(data1[0], null, 2));
        } else {
            console.log("Type: Dict");
            for (const [k, v] of Object.entries(data1)) {
                if (Array.isArray(v) && v.length > 0) {
                    console.log(`Key '${k}' is a list, First item:`, JSON.stringify(v[0], null, 2));
                } else {
                    console.log(`Key '${k}' type: ${typeof v}`);
                }
            }
        }
    } catch (e) {
        console.log("Error:", e);
    }

    try {
        console.log("\n--- Ihbarlar ---");
        const r2 = await fetch("https://acikveriapi.gaziantep.bel.tr/api/Itfaiye/Ihbarlar");
        const data2 = await r2.json();
        if (Array.isArray(data2)) {
            console.log("Type: List, Count:", data2.length);
            console.log("First item:", JSON.stringify(data2[0], null, 2));
        } else {
            console.log("Type: Dict");
            for (const [k, v] of Object.entries(data2)) {
                if (Array.isArray(v) && v.length > 0) {
                    console.log(`Key '${k}' is a list, First item:`, JSON.stringify(v[0], null, 2));
                } else {
                    console.log(`Key '${k}' type: ${typeof v}`);
                }
            }
        }
    } catch (e) {
        console.log("Error:", e);
    }
}
run();
