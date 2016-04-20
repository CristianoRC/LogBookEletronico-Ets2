﻿using System;
using System.Windows.Forms;

namespace NextteamBr
{
    public partial class Frm_Principal : Form
    {
        uint NumeroDeMultas = 0;
        bool inicioViajem = false;
        bool verificarDistanciaLocalDeEntrega = true;
        bool som5KMExecutado = false;
        bool executarAudio = true;
        Frete informacoesFrete = new Frete();
        RootObject informacoesGame;

        public Frm_Principal()
        {
            InitializeComponent();
        }

        private void Frm_Principal_Load(object sender, EventArgs e)
        {
            Ferramentas.LigarServidor();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            informacoesGame = ControllerTelemetry.ConvertJSON();

            Lbl_CidadeInicial.Text = informacoesGame.job.sourceCity;
            Lbl_CidadeDestino.Text = informacoesGame.job.destinationCity;
            Lbl_EmpresaInicial.Text = informacoesGame.job.sourceCompany;
            Lbl_EmpresaDestino.Text = informacoesGame.job.destinationCompany;

            Lbl_Cambio.Text = Convert.ToInt16(informacoesGame.truck.gear).ToString();
            Lbl_KMH.Text = Convert.ToInt16(informacoesGame.truck.speed).ToString();
            Lbl_KMRegistrado.Text = informacoesGame.truck.odometer.ToString("0.0");
            Lbl_RPM.Text = informacoesGame.truck.engineRpm.ToString("0.0");


            if (informacoesGame.game.paused == false)
            {
                VerificarCargaConectada();

                if (informacoesGame.trailer.attached)
                {
                    VerificarDistanciaEntrega();
                    VerificarCargaEntregue();
                    VerificarDanoDaCarga();
                }
            }
        }

        private void Btm_FreteCancelado_Click(object sender, EventArgs e)
        {
            Application.Restart();
        }

        private void Frm_Principal_FormClosing(object sender, FormClosingEventArgs e)
        {
            Ferramentas.DesligarServidor();
        }

        private void Btm_Iniciar_Click(object sender, EventArgs e)
        {
            timerVerificacaoGheral.Enabled = true;

            timerVerificacaoGheral.Start();

            Btm_FreteCancelado.Enabled = true;
            Btm_Iniciar.Enabled = false;
        }

        private void pictureSom_Click(object sender, EventArgs e)
        {
            if (executarAudio)
            {
                executarAudio = false;

                pictureSom.Image = NextteamBr.Properties.Resources.Mute_50;
            }
            else
            {
                executarAudio = true;

                pictureSom.Image = NextteamBr.Properties.Resources.Medium_Volume_50;
            }
        }

        private void Frm_Principal_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
                notifyIcon1.Visible = true;
                this.ShowInTaskbar = false;
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            notifyIcon1.Visible = false;
            this.ShowInTaskbar = false;
            this.WindowState = FormWindowState.Normal;
        }

        private void VerificarDanoDaCarga()
        {
            if (informacoesGame.trailer.wear > informacoesFrete.Dano)
            {
                informacoesFrete.Dano = (informacoesGame.trailer.wear + 0.08);

                if (executarAudio)
                {
                    ControllerAudio.ExecutarAudio("Dano");
                }
            }
        }

        private void VerificarCargaConectada()
        {

            timerLimitVelocidade.Start();

            if (inicioViajem == false)
            {
                if (informacoesGame.trailer.attached == true && informacoesGame.navigation.estimatedDistance > 300)
                {
                    inicioViajem = true;

                    if (executarAudio)
                    {
                        ControllerAudio.ExecutarAudio("Conectada");
                    }

                    Btm_FreteCancelado.Enabled = true;

                    informacoesFrete.DistanciaInicial = informacoesGame.truck.odometer;
                    informacoesFrete.Dano = informacoesGame.trailer.wear;
                }
            }
        }

        private void VerificarDistanciaEntrega()
        {
            if (verificarDistanciaLocalDeEntrega)
            {
                if (som5KMExecutado == false)
                {
                    if (informacoesGame.navigation.estimatedDistance <= 20000 && informacoesGame.navigation.estimatedDistance >= 19000) //2000 esta apenas usado para pequenas distancias..
                    {
                        verificarDistanciaLocalDeEntrega = false;

                        if (executarAudio)
                        {
                            ControllerAudio.ExecutarAudio("Distancia");
                        }

                        som5KMExecutado = true;
                    }
                }
            }
        }

        private void VerificarCargaEntregue()
        {
            if (informacoesGame.navigation.estimatedDistance <= 90 && informacoesGame.navigation.estimatedDistance >= 10)
            {
                if (executarAudio)
                {
                    ControllerAudio.ExecutarAudio("Entregue");
                }

                informacoesFrete.DistanciaFinal = informacoesGame.truck.odometer;

                informacoesFrete.KmRodado = informacoesFrete.DistanciaFinal - informacoesFrete.DistanciaInicial;

                informacoesFrete.KmRodado -= NumeroDeMultas * 1000;

                informacoesFrete.DataFinalFrete = DateTime.Now;

                SalvarCarga();

                timerLimitVelocidade.Stop();
            }
        }

        private void VerificarVelocidade()
        {
            if (informacoesGame.truck.speed > informacoesGame.navigation.speedLimit)
            {
                if (informacoesGame.truck.speed > ControleVelocidade.VelocidadeMaxima())
                {
                    NumeroDeMultas++;
                }
            }
        }

        private void SalvarCarga()
        {
            ControllerFrete.SalvarFrete(informacoesFrete, informacoesGame);

            inicioViajem = false;
            verificarDistanciaLocalDeEntrega = true;
            som5KMExecutado = false;
        }

        private void timerLimitVelocidade_Tick(object sender, EventArgs e)
        {
            VerificarVelocidade();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Frm_Config frm_Config = new Frm_Config();

            frm_Config.Show();
        }
    }
}
