using System;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace NDistribUnit.Common
{
    [ServiceContract(SessionMode=SessionMode.Required, 
        CallbackContract=typeof(ICallbacks))]
    public interface ITestRunner
    {
        [OperationContract]
        string GetData(int value);

        [OperationContract]
        CompositeType GetDataUsingDataContract(CompositeType composite);

        // TODO: Add your service operations here
    }

    [ServiceContract]
    public interface IWebEnabledContract
    {
        [OperationContract, WebGet]
        Stream GetWeb();
    }

    public class WebEnabledContract : IWebEnabledContract
    {

        public Stream GetWeb()
        {
            WebOperationContext.Current.OutgoingResponse.ContentType = "text/html";
            return new MemoryStream(Encoding.UTF8.GetBytes("<html><body><table><tr><td>Hello</td></tr><tr><td>world</td></tr></table></body></html>"));
        }
    }

    [DataContract]
    public class CompositeType
    {
        bool boolValue = true;
        string stringValue = "Hello ";

        [DataMember]
        public bool BoolValue
        {
            get { return boolValue; }
            set { boolValue = value; }
        }

        [DataMember]
        public string StringValue
        {
            get { return stringValue; }
            set { stringValue = value; }
        }
    }
}