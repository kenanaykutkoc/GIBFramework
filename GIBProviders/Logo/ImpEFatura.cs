﻿using GIBInterface;
using GIBInterface.UBLTR;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GIBProviders.Logo
{
    public partial class EFatura : IEFatura
    {
        public string ProviderId()
        {
            return "Logo";
        }

        public SendResult SendInvoice(SendParameters SendParameters)
        {

            SendResult r = new SendResult();

            var DocType =  GetDoc(Guid.NewGuid(), SendParameters);


            if (service.sendInvoice(DocType, "urn:mail:defaultpk@arkel.com.tr", SessionID))
            {
                r.IsSucceded = true;
                r.ResultInvoices = new List<ResultInvoice>();
                foreach (var item in SendParameters.InvoicesInfo)
                {
                    ResultInvoice rr = new ResultInvoice();
                    rr.ETN = item.Invoices.UUID.Value;
                    rr.FaturaNo = item.Invoices.ID.Value;
                    r.ResultInvoices.Add(rr);
                }

            }
            else
            {

            }


            return r;

        }

        public ServiceLogo.DocumentType GetDoc(Guid ZarfId, SendParameters SendParameters)
        {
            ServiceLogo.DocumentType DocType = new ServiceLogo.DocumentType();

            var zip = compressed(SendParameters);
            
            //TODO :buradaki file name için DB de Zarf ID oluşturulacak. henüz yapılmadı.
            DocType.fileName = ZarfId + ".zip";
            DocType.binaryData = new ServiceLogo.base64BinaryData();
            DocType.binaryData.Value = zip;
            DocType.hash = GetMd5Hash(zip) ;

            return DocType;
        }


        static string GetMd5Hash(byte[] input)
        {
            using (MD5 md5Hash = MD5.Create())
            {

                byte[] data = md5Hash.ComputeHash(input);
                StringBuilder sBuilder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }
                return sBuilder.ToString();
            }
        }


        public byte[] compressed(SendParameters SendParameters)
        {
            using (var compressedFileStream = new MemoryStream())
            {
                using (var zipArchive = new ZipArchive(compressedFileStream, ZipArchiveMode.Create, false))
                {
                    foreach (var invoice in SendParameters.InvoicesInfo)
                    {
                        var zipEntry = zipArchive.CreateEntry(invoice.Invoices.UUID.Value + ".xml");
                        byte[] bytes = invoice.Invoices.CreateBytes();
                        using (MemoryStream originalFileStream = new MemoryStream(bytes))
                        {
                            using (var zipEntryStream = zipEntry.Open())
                            {
                                originalFileStream.CopyTo(zipEntryStream);
                            }
                        }
                    }
                }
                return compressedFileStream.ToArray();
            }
        }


    }
}
