import requests
import json

url1 = "https://acikveriapi.gaziantep.bel.tr/api/Itfaiye/YanginNoktalari"
url2 = "https://acikveriapi.gaziantep.bel.tr/api/Itfaiye/Ihbarlar"

print("--- YanginNoktalari ---")
try:
    r1 = requests.get(url1)
    data1 = r1.json()
    if isinstance(data1, list):
        print(f"Type: List, Count: {len(data1)}")
        print("First item:", json.dumps(data1[0], indent=2, ensure_ascii=False))
    elif isinstance(data1, dict):
        print("Type: Dict")
        for k, v in data1.items():
            if isinstance(v, list) and len(v) > 0:
                print(f"Key '{k}' is a list, First item:", json.dumps(v[0], indent=2, ensure_ascii=False))
            else:
                print(f"Key '{k}' type: {type(v)}")
except Exception as e:
    print("Error:", e)

print("\n--- Ihbarlar ---")
try:
    r2 = requests.get(url2)
    data2 = r2.json()
    if isinstance(data2, list):
        print(f"Type: List, Count: {len(data2)}")
        print("First item:", json.dumps(data2[0], indent=2, ensure_ascii=False))
    elif isinstance(data2, dict):
        print("Type: Dict")
        for k, v in data2.items():
            if isinstance(v, list) and len(v) > 0:
                print(f"Key '{k}' is a list, First item:", json.dumps(v[0], indent=2, ensure_ascii=False))
            else:
                print(f"Key '{k}' type: {type(v)}")
except Exception as e:
    print("Error:", e)
