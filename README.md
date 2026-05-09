Okul: Sivas Cumhuriyet Üniversitesi
Proje: Unity Ortamında PID Kontrol ile Araba Şerit Takibi
Ders: Otomatik Kontrol
Öğrenci: Bertu Emir Deler (2023481022)

AMAÇ
Bu projenin temel amacı, Unity ortamında geliştirilen 3 boyutlu bir araç simülasyonu üzerinde, arabanın şerit merkez çizgisini (referans yörüngeyi) otonom olarak takip etmesini sağlayan bir geri beslemeli kontrol sisteminin tasarlanmasıdır. Proje kapsamında sadece düz hatlardan oluşmayan, farklı eğriliklerde ve en az 6 adet viraj içeren bir yol yapısı modellenmiş; aracın bu yörüngeden sapmasını minimize edecek P, PI ve PID kontrolcü algoritmaları C# diliyle sıfırdan kurgulanarak performans analizleri gerçekleştirilmiştir.

SİSTEM VE YOL TANIMI
Simülasyon, Unity 6 oyun motoru kullanılarak tasarlanmıştır. Yol modeli, Unity Splines paketi yardımıyla oluşturulmuş ve fiziksel zemin etkileşimi için sisteme Mesh Collider entegre edilmiştir. Rota, aracın eylemsizlik ve merkezkaç kuvvetlerine karşı kontrolcü performansının test edilebilmesi amacıyla hem düzlükler hem de hafif/keskin virajlar içerecek şekilde parametrik olarak bükülmüştür. Simülasyondaki araç, bir Rigidbody bileşeni ile kütle ve yerçekimi parametrelerine sahiptir. Sistemdeki referans değer olan $r(t)$, yol merkezinden geçen sanal spline hattıdır. 

SİSTEMİN MATEMATİKSEL YAPISI
1. Kinematik Model
Aracın kütlesinin (m) ve uygulanan kuvvetlerin ihmal edildiği, salt geometrik konum değişimini ifade eden modeldir. Aracın hızı V ve yönelim açısı θ olmak üzere sistemin durum denklemleri şu şekildedir:
ẋ = V cos(θ)
ż = V sin(θ)
θ̇ = ω
Bu modelde kontrolcü doğrudan açısal hızı (ω) kontrol ettiği için aracın kütlesi sistemin davranışını etkilememektedir.
2. Dinamik Model (Mevcut Sistem)
Unity’nin fizik motoru (Rigidbody) aktif edilerek (IsKinematic = False) aracın kütlesi (m) ve kütle atalet momenti (J) denkleme dahil edilmiştir. Bu modelde PID kontrolcüsü araca doğrudan açı vermek yerine, Y ekseni etrafında bir dönme momenti (Torque, τ) uygulamaktadır. Sistemin rotasyonel dinamiği Newton’un 2. Hareket Yasası’na göre şu şekildedir:
τ = Jθ̈ + bθ̇
Bu denklemde J kütle atalet momentini, θ̈ açısal ivmeyi ve b sönümleme/sürtünme katsayısını temsil etmektedir. Dinamik modelde J değeri kütleye doğrudan bağlı olduğu için:
J = ∫ r² dm
araç kütlesi değiştiğinde aynı yönlendiriyi izleyebilmek için kontrolcünün üretmesi gereken τ (Tork) miktarı da değişmek zorundadır.


HATA VE KONTROLCÜ YAPISI
Sistemde aracın referans yolun merkez hattına göre ölçülen yanal sapma miktarı, hata sinyali olan e(t) değerini oluşturmaktadır. Bu değer, aracın dünya (world) koordinatlarındaki konumunun, spline üzerindeki en yakın noktaya (y(t)) olan dik uzaklığının vektörel izdüşümü ile hesaplanmaktadır.
e(t) = r(t) - y(t)
Sistemi şeritte tutmak için uygulanan denetleyici, PID (Proportional-Integral-Derivative) mimarisindedir. Denetleyici çıkışı olan kontrol sinyali u(t), aşağıdaki denklemle hesaplanmaktadır:
u(t) = Kp·e(t) + Ki·∫e(t)dt + Kd·(de(t)/dt)
Hesaplanan bu u(t) kontrol sinyali, araca yönlendirme (steering) girdisi olarak verilmekte ve aracı referans merkeze yaklaştırmak için Y ekseni etrafında dönüş kuvveti uygulamaktadır.
Gerçek dünya fiziksel sınırlarını yansıtmak amacıyla, kontrol sinyali yazılım içerisinde ±1 (tam sağ / tam sol) doyum (saturation) limitlerine tabi tutulmuştur.

KULLANILAN PARAMETRELER
Testler sırasında aracın ileri yönlü hızı sabit (Speed = 5 birim) tutulmuş ve direksiyon hassasiyeti (Steering Sensitivity = 40) olarak belirlenmiştir. Karşılaştırma yapılabilmesi için üç farklı kontrolcü seti denenmiştir:
• P Kontrolcü:
Kp = 0.5, Ki = 0, Kd = 0
• PI Kontrolcü:
Kp = 0.5, Ki = 0.05, Kd = 0
• PID Kontrolcü (Optimum):
Kp = 0.5, Ki = 0, Kd = 0.1
(Not: Gerekirse kendi bulduğun en iyi değerleri buraya yazabilirsin.)

GRAFİKLER
(Bu bölüme Excel'den çıkardığın grafikleri ekran görüntüsü alarak eklemelisin. Her grafiğin altına şu açıklamaları yazabilirsin:)
•	Grafik 1: Referans ve Gerçek İzlenen Yol Karşılaştırması (Arabanın X ve Z koordinatları ile yolun X ve Z koordinatlarını üst üste çizdir.)
•	Grafik 2: P, PI ve PID Kontrolörler İçin Hata Değişimi (e(t))
•	Grafik 3: Optimum PID Seti İçin Kontrol Sinyali $u(t)) Çıkışı

KARŞILAŞTIRMA VE ANALİZLER
Simülasyon sonuçlarına ve grafik verilerine dayalı olarak sistemin davranışı aşağıdaki gibi analiz edilmiştir:
•	Araç düz yolda nasıl davranmaktadır? Düz yolda referans değişimi minimum olduğu için hata (e(t)) çok kısa sürede sıfıra yaklaşmakta ve araç, sıfır yanal sapma ile şeridin tam merkezine oturup stabil bir ilerleyiş sergilemektedir. 
•	Virajlarda hata nasıl değişmektedir? Viraj girişlerinde (eğriliğin aniden başladığı noktalarda) eylemsizlikten kaynaklı olarak aracın dışa savrulma eğilimi artmakta ve hata sinyali ani bir tepe noktasına (overshoot) ulaşmaktadır. Virajın ilerleyen bölümlerinde kontrolcü devreye girerek bu hatayı tekrar minimize etmektedir. 
•	PID parametreleri değiştirildiğinde araç davranışı nasıl etkilenmektedir? Sadece Oransal (Kp) kazanç kullanıldığında sistemin tepkisi hızlanmış ancak referans noktası etrafında ciddi salınımlar gözlemlenmiştir. İntegral (Ki) etkisi eklendiğinde viraj içindeki kalıcı hata azalsa da, viraj dönüşlerinde integral birikmesi (windup) yaşanarak geç tepkilere neden olmuştur. Türevsel (Kd) kazanç eklendiğinde ise gelecekteki hata öngörülerek salınımlar ciddi şekilde sönümlenmiş ve direksiyon tepkileri yumuşamıştır. 
•	Hangi parametre seti en iyi sonucu vermiştir? En başarılı şerit takibi ve en düşük salınım, Türevsel bileşenin aktif olduğu PID (veya PD ağırlıklı) parametre seti ile elde edilmiştir. 
•	Kontrol sinyali çok agresif midir, yumuşak mıdır? Kd bileşeninin sisteme dahil edilmesiyle birlikte kontrol sinyali u(t) yumuşatılmış ve keskin tepkilerden kaçınılmıştır. Sinyal yalnızca virajlara ilk girilen milisaniyelerde agresifleşerek doyum sınırlarına (+- 1) dayanmış, ardından hızlıca dengeye oturmuştur. 
•	Araç salınım yapmakta mıdır? Sistem kararlı mıdır? P ve PI kontrolcülerde araç referans çizgisini aşıp zikzaklar (salınım) çizmiştir. Ancak uygun türev (Kd) değeri ayarlandığında sistem sönümlü bir duruma geçmiş, salınımlar kaybolmuş ve sistem tamamen kararlı (stable) bir yapıya bürünmüştür. 
•	Keskin virajlarda takip başarımı düşmekte midir? Evet. Yörüngenin anlık değişim oranının (türevinin) çok yüksek olduğu keskin virajlarda, aracın fiziksel dönüş hızı limitli olduğundan hata miktarı kaçınılmaz olarak artmış ve referans izleme başarımı düz veya hafif virajlı bölümlere kıyasla nispeten düşmüştür. 
•	Yükselme ve yerleşme sürelerine ilişkin davranış nasıl etkilenmektedir? Oransal (Kp) değerinin artırılması sistemin yükselme süresini (referansa ilk ulaşma süresini) kısaltmış, ancak salınım yarattığı için yerleşme süresini uzatmıştır. Türev (Kd) parametresinin eklenmesi ise sönümleme yaparak yerleşme süresini (şeride tam ve pürüzsüz oturma anı) olağanüstü derecede kısaltmış ve kontrol performansını optimize etmiştir.
•	Aracın kütlesi (örneğin 1 ton ile 100 ton arasındaki fark) sistemin davranışını nasıl etkilemektedir? Kinematik modelden dinamik modele geçildiğinde kütlenin (m) sistem kararlılığı üzerindeki kritik rolü açıkça görülmüştür. 1 tonluk (1000 kg) standart bir araç için belirlenen kontrol sinyali (Tork çarpanı) ve Kp, Kd değerleri, araç 100 tona (100.000 kg) çıkarıldığında yetersiz kalmıştır. Kütlenin artmasıyla atalet momenti (J) devasa oranda büyümüş, aracın merkezkaç kuvvetine olan direnci kırılmış ve araç virajı alamayarak referans yörüngeden tamamen çıkmıştır. Bu durum, artan eylemsizliği yenebilmek (aracı döndürecek açısal ivmeyi elde edebilmek) için Dinamik Denklemdeki (τ = Jθ̈ + bθ̇) Tork ihtiyacının kütle ile doğru orantılı olarak arttığını kanıtlamıştır. 100 tonluk sistemi şeritte tutabilmek için kontrolcü kas gücünün (Tork çarpanı) ve hata tepkisinin (Kp, Kd kazançlarının) kütledeki artış oranına uygun olarak yeniden ayarlanması (tune edilmesi) gerekmiştir.


SONUÇ
Bu çalışmada, matematiksel bir PID kontrol algoritması Unity oyun motoru fiziklerine başarıyla entegre edilerek, gerçek zamanlı bir otonom sürüş simülasyonu elde edilmiştir. Deneyler, teorik denetleyici tasarımında P, I, D katsayılarının etkilerinin, sanal da olsa fiziksel bir sistemde birebir gözlemlenebildiğini kanıtlamıştır. Gelecek çalışmalarda, daha gelişmiş bulanık mantık (fuzzy logic) veya yapay sinir ağı tabanlı adaptif kontrolcülerin bu modele entegre edilmesi yörünge takip performansını daha da artıracaktır. 




