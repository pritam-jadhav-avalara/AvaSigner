using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;

namespace AvalaraReturnSigner
{
    [AspNetCompatibilityRequirements(RequirementsMode =
                             AspNetCompatibilityRequirementsMode.Allowed)]
    public class AvalaraReturnSigner : IAvalaraReturnSigner
    {
        InputSigner IAvalaraReturnSigner.Sign(InputSigner inputSigner)
        {
            try
            {
                InputSigner objBookDetails = new InputSigner();

                List<X509Certificate2> certificates = new List<X509Certificate2>();
                X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                X509Store store1 = new X509Store(StoreName.TrustedPublisher, StoreLocation.CurrentUser);
                store.Open(OpenFlags.OpenExistingOnly);
                foreach (X509Certificate2 cert in store.Certificates)
                {
                    certificates.Add(cert);
                }
                X509Certificate2 certificate = new X509Certificate2();
                X509Certificate2Collection cers = store.Certificates.Find(X509FindType.FindBySubjectName, inputSigner.ClientCertificateName, false);
                if (cers.Count > 0)
                    certificate = cers[0];
                else
                {
                    return new InputSigner()
                    {
                        Message = "Please connect your hardware digial signature (USB Token) Or Check the digital signature authority name you provided.",
                        ClientToolVersion = "1.0",
                        SignedPayload = string.Empty,
                        SignSucess = false,
                        ClientToolException = string.Empty

                    };
                }

                String text = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(inputSigner.SummaryPayload));
                ContentInfo contentInfo = new ContentInfo(System.Text.Encoding.UTF8.GetBytes(text));
                System.Security.Cryptography.Pkcs.SignedCms cms = new System.Security.Cryptography.Pkcs.SignedCms(contentInfo, false);
                CmsSigner signer = new CmsSigner(certificate);
                // signer.IncludeOption = X509IncludeOption.None;
                signer.DigestAlgorithm = new Oid("SHA256");
                cms.ComputeSignature(signer, false);
                byte[] signature = cms.Encode();

                return new InputSigner()
                {
                    Message = "Payload Signed Sucessfully",
                    SignedPayload = Convert.ToBase64String(signature),
                    SummaryPayload = inputSigner.SummaryPayload,
                    ClientToolVersion = "1.0",
                    SignSucess = true,
                    ClientToolException = string.Empty
                };
            }
            catch (Exception ex)
            {
                return new InputSigner()
                {
                    Message = "Payload Signed Sucessfylly",
                    SignedPayload = string.Empty,
                    SummaryPayload = inputSigner.SummaryPayload,
                    ClientToolVersion = "1.0",
                    SignSucess = true,
                    ClientToolException = ex.ToString()
                };
            }
        }
    }
}

public class MessageInspector : IDispatchMessageInspector
{
    private ServiceEndpoint _serviceEndpoint;

    public MessageInspector(ServiceEndpoint serviceEndpoint)
    {
        _serviceEndpoint = serviceEndpoint;
    }

    /// <summary>
    /// Called when an inbound message been received
    /// </summary>
    /// <param name="request">The request message.</param>
    /// <param name="channel">The incoming channel.</param>
    /// <param name="instanceContext">The current service instance.</param>
    /// <returns>
    /// The object used to correlate stateMsg. 
    /// This object is passed back in the method.
    /// </returns>
    public object AfterReceiveRequest(ref Message request,
                                          IClientChannel channel,
                                          InstanceContext instanceContext)
    {
        StateMessage stateMsg = null;
        HttpRequestMessageProperty requestProperty = null;
        if (request.Properties.ContainsKey(HttpRequestMessageProperty.Name))
        {
            requestProperty = request.Properties[HttpRequestMessageProperty.Name]
                              as HttpRequestMessageProperty;
        }

        if (requestProperty != null)
        {
            var origin = requestProperty.Headers["Origin"];
            if (!string.IsNullOrEmpty(origin))
            {
                stateMsg = new StateMessage();
                // if a cors options request (preflight) is detected, 
                // we create our own reply message and don't invoke any 
                // operation at all.
                if (requestProperty.Method == "OPTIONS")
                {
                    stateMsg.Message = Message.CreateMessage(request.Version, null);
                }
                request.Properties.Add("CrossOriginResourceSharingState", stateMsg);
            }
        }

        return stateMsg;
    }

    /// <summary>
    /// Called after the operation has returned but before the reply message
    /// is sent.
    /// </summary>
    /// <param name="reply">The reply message. This value is null if the 
    /// operation is one way.</param>
    /// <param name="correlationState">The correlation object returned from
    ///  the method.</param>
    public void BeforeSendReply(ref Message reply, object correlationState)
    {
        var stateMsg = correlationState as StateMessage;

        if (stateMsg != null)
        {
            if (stateMsg.Message != null)
            {
                reply = stateMsg.Message;
            }

            HttpResponseMessageProperty responseProperty = null;

            if (reply.Properties.ContainsKey(HttpResponseMessageProperty.Name))
            {
                responseProperty = reply.Properties[HttpResponseMessageProperty.Name]
                                   as HttpResponseMessageProperty;
            }

            if (responseProperty == null)
            {
                responseProperty = new HttpResponseMessageProperty();
                reply.Properties.Add(HttpResponseMessageProperty.Name,
                                     responseProperty);
            }

            // Access-Control-Allow-Origin should be added for all cors responses
            responseProperty.Headers.Set("Access-Control-Allow-Origin", "*");

            if (stateMsg.Message != null)
            {
                // the following headers should only be added for OPTIONS requests
                responseProperty.Headers.Set("Access-Control-Allow-Methods",
                                             "POST, OPTIONS, GET");
                responseProperty.Headers.Set("Access-Control-Allow-Headers",
                          "Content-Type, Accept, Authorization, x-requested-with");
            }
        }
    }
}

class StateMessage
{
    public Message Message;
}





public class BehaviorAttribute : Attribute, IEndpointBehavior,
                               IOperationBehavior
{
    public void Validate(ServiceEndpoint endpoint) { }

    public void AddBindingParameters(ServiceEndpoint endpoint,
                             BindingParameterCollection bindingParameters)
    { }

    /// <summary>
    /// This service modify or extend the service across an endpoint.
    /// </summary>
    /// <param name="endpoint">The endpoint that exposes the contract.</param>
    /// <param name="endpointDispatcher">The endpoint dispatcher to be
    /// modified or extended.</param>
    public void ApplyDispatchBehavior(ServiceEndpoint endpoint,
                                      EndpointDispatcher endpointDispatcher)
    {
        // add inspector which detects cross origin requests
        endpointDispatcher.DispatchRuntime.MessageInspectors.Add(
                                               new MessageInspector(endpoint));
    }

    public void ApplyClientBehavior(ServiceEndpoint endpoint,
                                    ClientRuntime clientRuntime)
    { }

    public void Validate(OperationDescription operationDescription) { }

    public void ApplyDispatchBehavior(OperationDescription operationDescription,
                                      DispatchOperation dispatchOperation)
    { }

    public void ApplyClientBehavior(OperationDescription operationDescription,
                                    ClientOperation clientOperation)
    { }

    public void AddBindingParameters(OperationDescription operationDescription,
                              BindingParameterCollection bindingParameters)
    { }

}