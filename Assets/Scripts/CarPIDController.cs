using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using System.IO;
using System.Text;

[RequireComponent(typeof(Rigidbody))]
public class CarPIDController : MonoBehaviour
{
    [Header("Referanslar")]
    public SplineContainer pathSpline;

    [Header("Dinamik Fizik Ayarlarý")]
    public float speed = 5f;
    [Tooltip("Kontrol sinyalini fiziksel dönme kuvvetine (Tork) çeviren çarpan")]
    public float torqueMultiplier = 2000f;

    [Header("PID Katsayýlarý")]
    public float Kp = 5.0f;
    public float Ki = 0.0f;
    public float Kd = 2.0f;

    [Header("Canlý Grafikler")]
    public AnimationCurve hataGrafigi_e = new AnimationCurve();
    public AnimationCurve kontrolSinyali_u = new AnimationCurve();

    private float integral = 0f;
    private float previousError = 0f;
    private Rigidbody rb;

    private string filePath;
    private StringBuilder csvData = new StringBuilder();
    private float timePassed = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        filePath = Application.dataPath + $"/PID_Grafik_{System.DateTime.Now:HH_mm_ss}.csv";
        csvData.AppendLine("Zaman(s);Hata_e(t);Kontrol_u(t);Araba_X;Araba_Z;Referans_X;Referans_Z");
    }

    void FixedUpdate()
    {
        if (pathSpline == null) return;

        timePassed += Time.fixedDeltaTime;

        // 1. ÝLERÝ YÖNLÜ DÝNAMÝK HAREKET (Arabanýn kendi motor gücü)
        // Y eksenindeki yerçekimi hýzýný koruyarak aracý ileri itiyoruz
        Vector3 forwardVel = transform.forward * speed;
        rb.linearVelocity = new Vector3(forwardVel.x, rb.linearVelocity.y, forwardVel.z);

        // 2. KORDÝNAT DÜZELTMESÝ 
        Vector3 localCarPos = pathSpline.transform.InverseTransformPoint(transform.position);
        float3 nearestPointLocal;
        float t;
        SplineUtility.GetNearestPoint(pathSpline.Spline, localCarPos, out nearestPointLocal, out t);
        Vector3 worldNearestPoint = pathSpline.transform.TransformPoint(nearestPointLocal);
        float3 localForward = SplineUtility.EvaluateTangent(pathSpline.Spline, t);
        Vector3 worldForward = pathSpline.transform.TransformDirection(localForward).normalized;

        if (Vector3.Dot(transform.forward, worldForward) < 0)
        {
            worldForward = -worldForward;
        }

        // 3. HATA (e(t)) HESABI
        Vector3 errorVector = transform.position - worldNearestPoint;
        Vector3 pathRightVector = Vector3.Cross(Vector3.up, worldForward).normalized;
        float currentError = Vector3.Dot(errorVector, pathRightVector);

        // 4. PID KONTROLCÜ
        integral += currentError * Time.fixedDeltaTime;
        float derivative = (currentError - previousError) / Time.fixedDeltaTime;
        float controlSignal = (Kp * currentError) + (Ki * integral) + (Kd * derivative);
        previousError = currentError;

        controlSignal = Mathf.Clamp(controlSignal, -1f, 1f);

        // 5. DÝNAMÝK YÖNLENDÝRME (TORK UYGULAMASI) - KÜTLENÝN ETKÝ ETTÝÐÝ YER
        // u(t) sinyalini bir fiziksel kuvvet momentine (Torque) çevirip arabanýn Y eksenine uyguluyoruz
        float appliedTorque = -controlSignal * torqueMultiplier;
        rb.AddTorque(transform.up * appliedTorque, ForceMode.Force);

        // Veri Kaydý
        hataGrafigi_e.AddKey(timePassed, currentError);
        kontrolSinyali_u.AddKey(timePassed, controlSignal);
        csvData.AppendLine($"{timePassed};{currentError};{controlSignal};{transform.position.x};{transform.position.z};{worldNearestPoint.x};{worldNearestPoint.z}");
    }

    void OnDestroy()
    {
        File.WriteAllText(filePath, csvData.ToString());
    }
}