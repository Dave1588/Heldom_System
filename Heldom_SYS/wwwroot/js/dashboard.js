// 等待 DOM 內容加載完成
document.addEventListener("DOMContentLoaded", function () {
    // 獲取所有觸發按鈕
    const triggers = document.querySelectorAll("#openPopupTrigger, #openPopupTrigger2, #openPopupTrigger3");
    // 獲取所有彈窗
    const popups = document.querySelectorAll("#openPopup, #openPopup2, #openPopup3");

    // 遍歷所有觸發按鈕，綁定點擊事件
    triggers.forEach((trigger, index) => {
        trigger.addEventListener("click", function () {
            console.log(`開啟彈窗 #${index + 1}`); // 測試事件是否觸發
            popups[index].style.display = "block"; // 顯示對應的彈窗
        });
    });

    // 遍歷所有彈窗，為每個彈窗的關閉按鈕綁定事件
    popups.forEach((popup) => {
        const closeButton = popup.querySelector(".close");
        if (closeButton) {
            closeButton.addEventListener("click", function () {
                console.log("關閉彈窗");
                popup.style.display = "none"; // 隱藏彈窗
            });
        }

        // 點擊彈窗外部時關閉該彈窗
        popup.addEventListener("click", function (event) {
            if (event.target === popup) {
                console.log("點擊彈窗外部，關閉彈窗");
                popup.style.display = "none";
            }
        });
    });
});


//// 等待 DOM 內容加載完成

//document.addEventListener("DOMContentLoaded", function () {
//    // 獲取觸發元素和彈窗元素
//    const trigger = document.querySelector("#openPopupTrigger"); // 觸發按鈕
//    const popup = document.querySelector("#openPopup");          // 彈窗
//    const closeButton = popup.querySelector(".close");           // 關閉按鈕

//    // 點擊觸發元素時顯示彈窗
//    trigger.addEventListener("click", function () {
//        popup.style.display = "block"; // 顯示彈窗
//    });

//    // 點擊關閉按鈕時隱藏彈窗
//    closeButton.addEventListener("click", function () {
//        popup.style.display = "none"; // 隱藏彈窗
//    });

//    // 可選：點擊彈窗外部時關閉彈窗
//    window.addEventListener("click", function (event) {
//        if (event.target === popup) {
//            popup.style.display = "none";
//        }
//    });
//});


//------------------------------------------------------------------

const WEATHER_API_KEY = 'CWA-78E720C0-B051-41F5-A176-58421841767B'; // 中央氣象署 API Key
const AIR_API_KEY = 'f9d919c8-5d1d-4787-b885-a96542a42aa2'; // 環境部空氣品質 API Key


//卡片動畫 上移
document.addEventListener("DOMContentLoaded", () => {
    document.querySelectorAll(".card").forEach((card, index) => {
        setTimeout(() => {
            card.classList.add("show");
        }, index * 250); // 讓卡片本身依序變半透明
    });
});

//IDW區域設置
const TARGET_DISTRICTS = [
    { name: '北區', lat: 24.165, lon: 120.675 },
    { name: '東區', lat: 24.145, lon: 120.695 },
    { name: '西區', lat: 24.145, lon: 120.655 },
    { name: '南區', lat: 24.125, lon: 120.665 },
    { name: '中區', lat: 24.137, lon: 120.678 },
    { name: '北屯區', lat: 24.191, lon: 120.706, stationId: 'C0F970' },
    { name: '西屯區', lat: 24.1629, lon: 120.6165, stationId: 'C0F9T0' },
    { name: '南屯區', lat: 24.133, lon: 120.6439, stationId: 'C0F9U0' }
];

// 計算兩點間距離（單位：公里）
function calculateDistance(lat1, lon1, lat2, lon2) {
    const R = 6371;
    const dLat = (lat2 - lat1) * Math.PI / 180;
    const dLon = (lon2 - lon1) * Math.PI / 180;
    const a = Math.sin(dLat / 2) * Math.sin(dLat / 2) +
        Math.cos(lat1 * Math.PI / 180) * Math.cos(lat2 * Math.PI / 180) *
        Math.sin(dLon / 2) * Math.sin(dLon / 2);
    const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
    return R * c;
}

// IDW 插值計算
function interpolateValue(targetLat, targetLon, stations, key) {
    let numerator = 0, denominator = 0;
    stations.forEach(station => {
        const wgs84 = station.GeoInfo.Coordinates.find(c => c.CoordinateName === 'WGS84');
        const distance = calculateDistance(targetLat, targetLon, wgs84.StationLatitude, wgs84.StationLongitude);
        if (distance === 0) return;
        const weight = 1 / Math.pow(distance, 2);
        const value = key === 'rain' ? station.WeatherElement.Now.Precipitation : station.WeatherElement[key];
        numerator += value * weight;
        denominator += weight;
    });
    return parseFloat((numerator / denominator).toFixed(1));
}

// 轉換風速為蒲福氏風力等級
function convertToBeaufort(windSpeed) {
    windSpeed = Math.abs(windSpeed); // 取絕對值，修正負數
    if (windSpeed <= 0.2) return 0;
    if (windSpeed <= 1.5) return 1;
    if (windSpeed <= 3.3) return 2;
    if (windSpeed <= 5.4) return 3;
    if (windSpeed <= 7.9) return 4;
    if (windSpeed <= 10.7) return 5;
    if (windSpeed <= 13.8) return 6;
    if (windSpeed <= 17.1) return 7;
    if (windSpeed <= 20.7) return 8;
    if (windSpeed <= 24.4) return 9;
    if (windSpeed <= 28.4) return 10;
    if (windSpeed <= 32.6) return 11;
    return 12; // ≥32.7 m/s
}

// 更新元素，針對風速顯示級數並根據原始值切換顏色
function updateElement(id, value, thresholds, unit, rawValue = null) {
    const element = document.getElementById(id);
    if (!element) {
        console.error(`未找到 ID 為 ${id} 的元素`);
        return;
    }

    if (id === 'wind' && rawValue !== null) {
        const windLevel = convertToBeaufort(rawValue);
        element.innerHTML = `<p class="value-text">${windLevel}</p><p class="unit-text">${unit}</p>`;
        element.classList.remove('safe', 'warning', 'danger');
        if (rawValue <= thresholds.safe) element.classList.add('safe');
        else if (rawValue <= thresholds.warning) element.classList.add('warning');
        else element.classList.add('danger');
    } else {
        element.innerHTML = `<p class="value-text">${value}</p><p class="unit-text">${unit}</p>`;
        element.classList.remove('safe', 'warning', 'danger');
        if (value <= thresholds.safe) element.classList.add('safe');
        else if (value <= thresholds.warning) element.classList.add('warning');
        else element.classList.add('danger');
    }
    const descElement = document.getElementById(`${id}-desc`);
    if (descElement) descElement.textContent = getDescription(id, value);
}
// 描述邏輯
function getDescription(id, value) {
    switch (id) {
        case 'temp': return value > 35 ? '極端高溫，防中暑' : '正常';
        case 'humidity': return value > 80 ? '過高，影響施工' : '正常';
        case 'wind': {
            const beaufort = convertToBeaufort(value);
            if (beaufort <= 4) return `${beaufort}級 - 正常`;
            if (beaufort <= 6) return `${beaufort}級 - 注意`;
            return `${beaufort}級 - 危險，應暫停吊掛`;
        }
        case 'rain': return value > 10 ? '影響結構安全' : '正常';
        case 'pm25': return value > 50 ? '紅色警戒，建議戴N95口罩' : (value > 30 ? '黃色警戒' : '正常');
        case 'noise': return value > 80 ? '超標，需工安管制' : (value > 65 ? '注意' : '正常');
        default: return '未知';
    }
}

// 自動偵測目前區域 如定位失敗會使用預設位置
function getCurrentDistrict(callback) {
    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(position => {
            const { latitude, longitude } = position.coords;
            let closestDistrict = null;
            let minDistance = Infinity;
            TARGET_DISTRICTS.forEach(district => {
                const distance = calculateDistance(latitude, longitude, district.lat, district.lon);
                if (distance < minDistance) {
                    minDistance = distance;
                    closestDistrict = district;
                }
            });
            callback(closestDistrict);
        }, error => {
            console.error('地理定位失敗:', error);
            document.getElementById('weather-status').textContent = '無法偵測位置，使用預設區域 (中區)';
            callback(TARGET_DISTRICTS.find(d => d.name === '中區'));
        });
    } else {
        console.error('瀏覽器不支援地理定位');
        callback(TARGET_DISTRICTS.find(d => d.name === '中區'));
    }
}

// 麥克風噪音監測
async function startNoiseMonitoring() {
    try {
        const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
        const audioContext = new (window.AudioContext || window.webkitAudioContext)();
        const analyser = audioContext.createAnalyser();
        const microphone = audioContext.createMediaStreamSource(stream);
        microphone.connect(analyser);
        analyser.fftSize = 2048;

        const dataArray = new Uint8Array(analyser.frequencyBinCount);
        function updateNoise() {
            analyser.getByteTimeDomainData(dataArray);
            let sum = 0;
            for (let i = 0; i < dataArray.length; i++) {
                const value = (dataArray[i] - 128) / 128;
                sum += value * value;
            }
            const rms = Math.sqrt(sum / dataArray.length);
            const db = 20 * Math.log10(rms) + 90;
            const noise = Math.max(0, Math.round(db));
            updateElement('noise', noise, { safe: 65, warning: 80 }, 'dB');
        }
        updateNoise();
        setInterval(updateNoise, 3000);
    } catch (error) {
        console.error('麥克風監測失敗:', error);
        updateElement('noise', 0, { safe: 65, warning: 80 }, 'dB');
    }
}

// 獲取並顯示數據
async function fetchAndDisplayWeather() {
    getCurrentDistrict(async (currentDistrict) => {
        try {
            const weatherResponse = await fetch(
                `https://opendata.cwa.gov.tw/api/v1/rest/datastore/O-A0001-001?Authorization=${WEATHER_API_KEY}`
            );
            const weatherData = await weatherResponse.json();
            

            //---------------------------------
            let pm25;
            try {
                const airResponse = await fetch(
                    `https://data.moenv.gov.tw/api/v2/aqx_p_432?api_key=${AIR_API_KEY}`
                );
                const airData = await airResponse.json();

                if (!airData.records || !Array.isArray(airData.records))
                    throw new Error('空氣品質 API 無有效數據');

                console.log('API 回傳數據:', weatherData.records);   

                 console.log('API 回傳數據:', airData.records);   

                const westStation = airData.records.find(
                    s => s.sitename === '西屯' && s.county === '臺中市'
                );

                if (westStation) {
                    const pm25Value = westStation["pm2.5"] || westStation["pm2_5"] || null;
                    if (pm25Value !== null) {
                        pm25 = parseFloat(pm25Value);
                        if (isNaN(pm25)) throw new Error('PM2.5 數據格式錯誤');
                    } else {
                        console.warn('西屯 PM2.5 數據缺失');
                        pm25 = 0;
                    }
                } else {
                    console.warn('找不到站，');
                    pm25 = 0;
                }

                //console.log('西屯 PM2.5:', pm25);
            } catch (airError) {
                console.error('PM2.5 數據獲取失敗:', airError);
                pm25 = 25; // API 錯誤時也使用預設值，避免 NaN
            }

            if (weatherData.success !== 'true') throw new Error('天氣 API 失敗');
            const weatherStations = weatherData.records.Station.filter(s => s.GeoInfo.CountyName === '臺中市');
            let temp, humidity, wind, rain;

            if (currentDistrict.stationId) {
                const station = weatherStations.find(s => s.StationId === currentDistrict.stationId);
                if (station) {
                    temp = station.WeatherElement.AirTemperature;
                    humidity = station.WeatherElement.RelativeHumidity;
                    wind = Math.abs(station.WeatherElement.WindSpeed); // 取絕對值
                    rain = station.WeatherElement.Now.Precipitation < 0 ? 0 : station.WeatherElement.Now.Precipitation;
                }
            } else {
                temp = interpolateValue(currentDistrict.lat, currentDistrict.lon, weatherStations, 'AirTemperature');
                humidity = interpolateValue(currentDistrict.lat, currentDistrict.lon, weatherStations, 'RelativeHumidity');
                wind = Math.abs(interpolateValue(currentDistrict.lat, currentDistrict.lon, weatherStations, 'WindSpeed')); // 取絕對值
                rain = interpolateValue(currentDistrict.lat, currentDistrict.lon, weatherStations, 'rain');
                rain = rain < 0 ? 0 : rain;
            }

            const windLevel = convertToBeaufort(wind); // 轉為級風

            document.getElementById('weather-icon').textContent = rain > 0 ? '🌧️' : '☀️';
            document.getElementById('weather-status').textContent = `${currentDistrict.name} - ${rain > 0 ? '中雨' : '晴天'}`;

            updateElement('temp', temp, { safe: 26, warning: 30 }, '°C');
            updateElement('humidity', humidity, { safe: 60, warning: 80 }, '%');
            updateElement('wind', wind, { safe: 5, warning: 7 }, '級', wind); // 傳入原始 m/s 值
            updateElement('rain', rain, { safe: 50, warning: 200 }, 'mm');
            updateElement('pm25', pm25, { safe: 35, warning: 50 }, 'μg/m³');

            startNoiseMonitoring();
        } catch (error) {
            console.error('天氣數據載入失敗:', error);
            document.getElementById('weather-status').textContent = '數據載入失敗';
        }
    });
}

fetchAndDisplayWeather();
setInterval(fetchAndDisplayWeather, 300000);



