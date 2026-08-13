# KML Veri Dosyaları

Bu klasöre aşağıdaki KML dosyalarını koyun:

| Dosya | İçerik |
|---|---|
| `districts.kml` | Gaziantep ilçe sınırları (Polygon Placemarks) |
| `neighborhoods.kml` | Mahalle sınırları (Polygon Placemarks) |
| `stations.kml` | İtfaiye istasyonları (Point Placemarks) |

Dosyalar bulunamazsa sistem otomatik olarak mock verilerle çalışmaya devam eder.

## Beklenen KML Formatı (Örnek)

```xml
<?xml version="1.0" encoding="UTF-8"?>
<kml xmlns="http://www.opengis.net/kml/2.2">
  <Document>
    <Placemark>
      <name>Şahinbey</name>
      <Polygon>
        <outerBoundaryIs>
          <LinearRing>
            <coordinates>37.30,37.00,0 37.38,37.00,0 37.38,37.06,0 37.30,37.06,0 37.30,37.00,0</coordinates>
          </LinearRing>
        </outerBoundaryIs>
      </Polygon>
    </Placemark>
  </Document>
</kml>
```
