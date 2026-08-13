async function run() {
    try {
        const r2 = await fetch("https://acikveriapi.gaziantep.bel.tr/api/Itfaiye/Ihbarlar");
        const data2 = await r2.json();
        console.log("Ihbarlar data keys:", Object.keys(data2));
        if (data2.data) {
           console.log("data2.data type:", typeof data2.data);
           if (Array.isArray(data2.data)) {
               console.log("data2.data length:", data2.data.length);
               console.log("first item:", JSON.stringify(data2.data[0], null, 2));
           } else {
               console.log("data keys:", Object.keys(data2.data));
           }
        }
    } catch (e) {
        console.log("Error:", e);
    }
}
run();
