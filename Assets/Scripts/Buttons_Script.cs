using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using XCharts.Runtime;

public class Buttons_Script : MonoBehaviour
{

    public GameObject deLabel;
    public GameObject ateLabel;

    public TextMeshPro dateResult;
    public TextMeshPro amountResult;
    public TextMeshPro codeResult;
    public TextMeshPro symbolResult;
    public TextMeshPro priceResult;
    public TextMeshPro totalResult;

    public TextMeshPro iaText;
    private DateTime dateDe;
    private DateTime dateAte;
    private string textDe;
    private string textAte;

    //https://blog.csdn.net/lizishishui/article/details/134952150
    public GameObject pieChart;

    public GameObject quadResult;

    [System.Serializable]
    public struct AnalysedObject
    {
        public Document document;
    }

    [System.Serializable]
    public struct Document
    {
        public string _id;
        public int account_id;
        public int transaction_count;
        public string bucket_start_date;
        public string bucket_end_date;
        public List<Transactions> transactions;
    }

    [System.Serializable]
    public struct Transactions
    {
        public string date;
        public int amount;
        public string transaction_code;
        public string symbol;
        public string price;
        public string total;
    }


    // Start is called before the first frame update
    void Start()
    {
        quadResult.SetActive(false);
        iaText.text = "";
    }

    // Update is called once per frame
    void Update()
    {

        
    }

    public void OnButtonProcurarClick()
    {
        Debug.Log("Button Procurar is pressed");

        //Zera
        iaText.text = "";
        quadResult.SetActive(false);
        dateResult.text = "Date Result";
        amountResult.text = "Amount Result";
        codeResult.text = "Code Result";
        symbolResult.text = "Symbol Result";
        priceResult.text = "Price Result";
        totalResult.text = "Total Result";

        //IA date
        textDe = deLabel.GetComponent<TMP_Text>().text;
        Debug.Log("De string: " + textDe);

        textAte = ateLabel.GetComponent<TMP_Text>().text;
        Debug.Log("Ate string: " + textAte);

        if (textDe != "Select a Date")
        {
            dateDe = DateTime.Parse(textDe);
        }
        else
        {
            dateDe = DateTime.MinValue;
        }

        if (textAte != "Select a Date")
        {
            dateAte = DateTime.Parse(textAte);
        }
        else
        {
            dateAte = DateTime.MaxValue;
        }

        Debug.Log("De datetime: " + dateDe);
        Debug.Log("Ate datetime: " + dateAte);


        if (textDe != "Select a Date" & textAte != "Select a Date" & dateAte < dateDe)
        {
            iaText.text = "Data Até é menor que Data De por favor colocar Data Até maior ou igual a Data De";
        }
        else
        {
            StartCoroutine(GetTransactions2());
        }
    }

    public void OnButtonPDFClick()
    {
        Debug.Log("Button PDF is pressed");
        //Não achei asset free para criar e visualizar pdf
        //https://assetstore.unity.com/packages/tools/gui/pdf-renderer-32815
    }

    public void OnButtonChartClick()
    {
        Debug.Log("Button Chart is pressed");
        //sendo feito automatico após criar lista
        
        var chart = pieChart.GetComponent<PieChart>();
        //Amount
        chart.UpdateData(0, 0, 50);

        //Price
        chart.UpdateData(0, 1, 40);

        //Total
        chart.UpdateData(0, 2, 30);

    }

    public IEnumerator GetTransactions2()
    {
        WWWForm webForm = new WWWForm();
        using (UnityWebRequest unityWebRequest = UnityWebRequest.Post("YOUR MONGODB ENDPOINT", webForm))
        {

            unityWebRequest.SetRequestHeader("Content-Type", "application/json");
            unityWebRequest.SetRequestHeader("Access-Control-Request-Headers", "*");
            unityWebRequest.SetRequestHeader("api-key", "YOUR MONGODB API KEY");

            unityWebRequest.downloadHandler = new DownloadHandlerBuffer();

            string jsonBody = "{\"collection\":\"transactions\",\"database\":\"sample_analytics\",\"dataSource\":\"Cluster0\",\"filter\": {\"bucket_start_date\": { \"$date\": \"1969-02-04T00:00:00Z\"}}}";
            byte[] bytes = Encoding.UTF8.GetBytes(jsonBody);

            unityWebRequest.uploadHandler = new UploadHandlerRaw(bytes);
            unityWebRequest.uploadHandler.contentType = "application/json";

            yield return unityWebRequest.SendWebRequest();

            long responseCode = unityWebRequest.responseCode;

            try
            {
                string jsonResponse = null;
                jsonResponse = unityWebRequest.downloadHandler.text;

                Debug.Log("jsonResponse is: " + jsonResponse);

                AnalysedObject analysedObject = new AnalysedObject();
                analysedObject = JsonUtility.FromJson<AnalysedObject>(jsonResponse);

                var totalAmount = 0;
                var totalPrice = 0;
                var totalTotal = 0;

                foreach (Transactions trans in analysedObject.document.transactions)
                {
                    Debug.Log("Transaction Date is: " + trans.date);

                    var onlyDate = trans.date.Substring(0, 10);
                    Debug.Log("Apenas Data: " + onlyDate);

                    var transDateData = DateTime.Parse(onlyDate);
                    Debug.Log("transaction Date Formato Data: " + transDateData);

                    Debug.Log(dateDe + "<=" + transDateData + "<=" + dateAte);

                    //faixa de ate
                    if (transDateData >= dateDe & transDateData <= dateAte)
                    {

                        if (dateResult.text == "Date Result")
                        {
                            dateResult.text = onlyDate + "\n";
                        }
                        else
                        {
                            dateResult.text = dateResult.text + onlyDate + "\n";
                        }

                        Debug.Log("Transaction amount is: " + trans.amount);
                        if (amountResult.text == "Amount Result")
                        {
                            amountResult.text = trans.amount + "\n";
                        }
                        else
                        {
                            amountResult.text = amountResult.text + trans.amount + "\n";
                        }

                        Debug.Log("Transaction Code is: " + trans.transaction_code);
                        if (codeResult.text == "Code Result")
                        {
                            codeResult.text = trans.transaction_code + "\n";
                        }
                        else
                        {
                            codeResult.text = codeResult.text + trans.transaction_code + "\n";
                        }

                        Debug.Log("Transaction Symbol is: " + trans.symbol);
                        if (symbolResult.text == "Symbol Result")
                        {
                            symbolResult.text = trans.symbol + "\n";
                        }
                        else
                        {
                            symbolResult.text = symbolResult.text + trans.symbol + "\n";
                        }

                        Debug.Log("Transaction Price is: " + trans.price);

                        //Only integer
                        int substringStartIndexPrice = trans.price.IndexOf(".");
                        string onlyIntPrice = trans.price.Substring(0, substringStartIndexPrice);
                        Debug.Log("Only Int Price is: " + onlyIntPrice);

                        if (priceResult.text == "Price Result")
                        {
                            priceResult.text = onlyIntPrice + "\n";
                        }
                        else
                        {
                            priceResult.text = priceResult.text + onlyIntPrice + "\n";
                        }

                        Debug.Log("Transaction total is: " + trans.total);

                        //Only integer
                        var substringStartIndexTotal = trans.total.IndexOf(".");
                        var onlyIntTotal = trans.total.Substring(0, substringStartIndexTotal);
                        Debug.Log("Only Int Total is: " + onlyIntTotal);

                        if (totalResult.text == "Total Result")
                        {
                            totalResult.text = onlyIntTotal + "\n";
                        }
                        else
                        {
                            totalResult.text += onlyIntTotal + "\n";
                        }

                        //Totais
                        totalAmount += trans.amount;
                        totalPrice += Int32.Parse(onlyIntPrice);
                        totalTotal += Int32.Parse(onlyIntTotal);


                    }

                }

                Debug.Log("Total Amout is: " + totalAmount);
                Debug.Log("Total Price is: " + totalPrice);
                Debug.Log("Total Total is: " + totalTotal);

                //100%
                var allTotal = totalAmount + totalPrice + totalTotal;

                var pieAmount = (totalAmount*100)/allTotal;
                var piePrice = (totalPrice * 100) / allTotal;
                var pieTotal = (totalTotal * 100) / allTotal;

                if(pieAmount == 0)
                {
                    pieAmount = 1;
                }

                if(piePrice == 0)
                {
                    piePrice = 1;
                }

                if(pieTotal == 0)
                {
                    pieTotal = 1;
                }

                Debug.Log("Pie Amount is: " + pieAmount);
                Debug.Log("Pie Price is: " + piePrice);
                Debug.Log("Pie Total is: " + pieTotal);

                var pieTotalGeral = pieAmount + piePrice + pieTotal;
                Debug.Log("Pie Amount + Pie Price + PieTotal is: " + pieTotalGeral);

                var chart = pieChart.GetComponent<PieChart>();
                //Amount
                chart.UpdateData(0, 0, pieAmount);

                //Price
                chart.UpdateData(0, 1, piePrice);

                //Total
                chart.UpdateData(0, 2, pieTotal);

            }
            catch (Exception exception)
            {
                Debug.Log("exception.Message: " + exception.Message);
                iaText.text = "exception.Message: " + exception.Message;
            }

            if (dateResult.text == "Date Result")
            {
                iaText.text = "Não foi encontrado nenhum registro entre a faixa de data";
            } 
            else
            {
                quadResult.SetActive(true);
            }

            yield return null;
        }

    }

}
