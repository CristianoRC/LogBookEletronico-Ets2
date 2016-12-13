﻿using System;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace NextteamBr
{
    public static class ControleVersao
    {
        public static String VerificarAtualizacoes()
        {
            String Saida;

            try
            {
                string versao, Link;

                System.Net.HttpWebRequest request =
                (HttpWebRequest)WebRequest.Create(@"http://cristianoprogramador.com/LogBook/Versao.txt");

                WebResponse response = request.GetResponse();

                using (Stream stream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                    string[] temp = (reader.ReadToEnd()).Split(';');

                    versao = temp[0];
                    Link = temp[1];
                }

                if (versao != Application.ProductVersion)
                {
                    Saida = Link;
                }
                else
                {
                    Saida = string.Empty;
                }
            }
            catch (System.Exception ex)
            {
                Saida = string.Format("Erro ao tentar fazer a requisição: {0}", ex.Message);
            }

            return Saida;
        }
    }
}