using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using System.IO;
using System.Text;

public class CarPIDController : MonoBehaviour
{
    [Header("Referanslar")]
    public SplineContainer pathSpline;

    [Header("Araç Ayarlarý")]
    public float speed = 5f;
    public float steeringSensitivity = 40f;

    [Header("PID Katsayýlarý")]
    public float Kp = 0.5f;
    public float Ki = 0.0f;
    public float Kd = 0.1f;

    [Header("Canlý Grafikler (Oynatýrken Týkla)")]
    [Tooltip("Play modundayken bu yeþil kutulara týklayarak grafiði canlý izleyebilirsin.")]
    public AnimationCurve hataGrafigi_e = new AnimationCurve();
    public AnimationCurve kontrolSinyali_u = new AnimationCurve();

    private float integral = 0f;
    private float previousError = 0f;

    // --- VERÝ KAYDETME DEÐÝÞKENLERÝ ---
    private string filePath;
    private StringBuilder csvData = new StringBuilder();
    private float timePassed = 0f;

    void Start()
    {
        // Dosya ismine tarih ve saat ekleyerek her testte yeni bir dosya oluþturur
        filePath = Application.dataPath + $"/PID_Grafik_{System.DateTime.Now:HH_mm_ss}.csv";
        csvData.AppendLine("Zaman(s);Hata_e(t);Kontrol_u(t);Araba_X;Araba_Z;Referans_X;Referans_Z");
    }

    void FixedUpdate()
    {
        if (pathSpline == null) return;

        timePassed += Time.fixedDeltaTime;

        // 1. ARACI ÝLERÝ SÜR
        transform.Translate(Vector3.forward * speed * Time.fixedDeltaTime);

        // 2. KORDÝNAT DÜZELTMESÝ 
        Vector3 localCarPos = pathSpline.transform.InverseTransformPoint(transform.position);
        float3 nearestPointLocal;
        float t;
        SplineUtility.GetNearestPoint(pathSpline.Spline, localCarPos, out nearestPointLocal, out t);
        Vector3 worldNearestPoint = pathSpline.transform.TransformPoint(nearestPointLocal);
        float3 localForward = SplineUtility.EvaluateTangent(pathSpline.Spline, t);
        Vector3 worldForward = pathSpline.transform.TransformDirection(localForward).normalized;

        // 3. TERS YÖN KORUMASI 
        if (Vector3.Dot(transform.forward, worldForward) < 0)
        {
            worldForward = -worldForward;
        }

        // 4. HATA (e(t)) HESABI
        Vector3 errorVector = transform.position - worldNearestPoint;
        Vector3 pathRightVector = Vector3.Cross(Vector3.up, worldForward).normalized;
        float currentError = Vector3.Dot(errorVector, pathRightVector);

        // 5. PID KONTROLCÜ VE DENKLEM
        integral += currentError * Time.fixedDeltaTime;
        float derivative = (currentError - previousError) / Time.fixedDeltaTime;
        float controlSignal = (Kp * currentError) + (Ki * integral) + (Kd * derivative);
        previousError = currentError;

        // Sinyal Doyumu
        controlSignal = Mathf.Clamp(controlSignal, -1f, 1f);

        // 6. DÝREKSÝYONU KIR
        transform.Rotate(Vector3.up, -controlSignal * steeringSensitivity * Time.fixedDeltaTime);

        // --- 7. CANLI GRAFÝÐE VERÝ GÖNDERME ---
        hataGrafigi_e.AddKey(timePassed, currentError);
        kontrolSinyali_u.AddKey(timePassed, controlSignal);

        // --- 8. EXCEL ÝÇÝN HAFIZAYA YAZDIR ---
        csvData.AppendLine($"{timePassed};{currentError};{controlSignal};{transform.position.x};{transform.position.z};{worldNearestPoint.x};{worldNearestPoint.z}");
    }

    void OnDestroy()
    {
        File.WriteAllText(filePath, csvData.ToString());
        Debug.Log("Grafik verileri baþarýyla kaydedildi! Dosya yolu: " + filePath);
    }
}