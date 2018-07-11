// Author:
//       Brian Faust <brian@ark.io>
//
// Copyright (c) 2018 Ark Ecosystem <info@ark.io>
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using NBitcoin.DataEncoders;
using System;
using System.IO;

namespace ArkEcosystem.Crypto
{
    public class Serialiser
    {
        readonly TransactionModel transaction;
        readonly MemoryStream stream;
        readonly BinaryWriter writer;

        public Serialiser(TransactionModel transaction)
        {
            this.transaction = transaction;
            stream = new MemoryStream();
            writer = new BinaryWriter(stream);
        }

        public byte[] Serialise()
        {
            HandleHeader();
            HandleTypeSpecific();
            HandleSignatures();

            return stream.ToArray();
        }

        public void HandleHeader()
        {
            writer.Write(transaction.Header);
            writer.Write(transaction.Version);
            writer.Write(transaction.Network);
            writer.Write(transaction.Type);
            writer.Write(transaction.Timestamp);
            writer.Write(Encoders.Hex.DecodeData(transaction.SenderPublicKey));
            writer.Write(transaction.Fee);

            if (transaction.VendorField != null)
            {
                // writer.Write((byte)transaction.VendorField.Length);
                writer.Write(transaction.VendorField);
            }
            else if (transaction.VendorFieldHex != null)
            {
                writer.Write((byte)(transaction.VendorFieldHex.Length / 2));
                writer.Write(transaction.VendorFieldHex);
            }
            else
            {
                writer.Write((byte)0x00);
            }
        }

        private void HandleTypeSpecific()
        {
            switch (transaction.Type)
            {
                case 0:
                    Serialisers.Transfer.Serialise(writer, transaction);
                    break;
                case 1:
                    Serialisers.SecondSignatureRegistration.Serialise(writer, transaction);
                    break;
                case 2:
                    Serialisers.DelegateRegistration.Serialise(writer, transaction);
                    break;
                case 3:
                    Serialisers.Vote.Serialise(writer, transaction);
                    break;
                case 4:
                    Serialisers.MultiSignatureRegistration.Serialise(writer, transaction);
                    break;
                case 5:
                    Serialisers.IPFS.Serialise(writer, transaction);
                    break;
                case 6:
                    Serialisers.TimelockTransfer.Serialise(writer, transaction);
                    break;
                case 7:
                    Serialisers.MultiPayment.Serialise(writer, transaction);
                    break;
                case 8:
                    Serialisers.DelegateResignation.Serialise(writer, transaction);
                    break;
            }
        }

        void HandleSignatures()
        {
            if (transaction.Signature != null)
            {
                writer.Write(Encoders.Hex.DecodeData(transaction.Signature));
            }

            if (transaction.SecondSignature != null)
            {
                writer.Write(Encoders.Hex.DecodeData(transaction.SecondSignature));
            }
            else if (transaction.SignSignature != null)
            {
                writer.Write(Encoders.Hex.DecodeData(transaction.SignSignature));
            }

            if (transaction.Signatures != null)
            {
                writer.Write((byte)0xff);
                writer.Write(Encoders.Hex.DecodeData(String.Join("", transaction.Signatures)));
            }
        }
    }
}