using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.ServiceModel.Web;


namespace AvalaraReturnSigner
{
    [ServiceContract()]
    public interface IAvalaraReturnSigner
    {
        [OperationContract()]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        InputSigner Sign(InputSigner sBookID);
    }

    [DataContract()]
    public class InputSigner
    {
        string _summaryPayload;
        string _signedPayload;
        string _clientToolVersion;
        string _serverToolVersion;
        string _message;
        string _clientCertificateName;
        bool _SignSucess;
        string _ClientToolexception;


        int _iId;

        [DataMember()]
        public string SummaryPayload
        {
            get { return _summaryPayload; }
            set { _summaryPayload = value; }
        }

        [DataMember()]
        public string SignedPayload
        {
            get
            {
                return _signedPayload;
            }

            set
            {
                _signedPayload = value;
            }
        }

        [DataMember()]
        public string ClientToolVersion
        {
            get
            {
                return _clientToolVersion;
            }

            set
            {
                _clientToolVersion = value;
            }
        }

        [DataMember()]
        public string ServerToolVersion
        {
            get
            {
                return _serverToolVersion;
            }

            set
            {
                _serverToolVersion = value;
            }
        }
        [DataMember()]
        public string Message
        {
            get
            {
                return _message;
            }

            set
            {
                _message = value;
            }
        }

        [DataMember()]
        public string ClientCertificateName
        {
            get
            {
                return _clientCertificateName;
            }

            set
            {
                _clientCertificateName = value;
            }
        }

        [DataMember()]
        public bool SignSucess
        {
            get
            {
                return _SignSucess;
            }

            set
            {
                _SignSucess = value;
            }
        }

        [DataMember()]
        public string ClientToolException
        {
            get
            {
                return _ClientToolexception;
            }

            set
            {
                _ClientToolexception = value;
            }
        }
    }


}
