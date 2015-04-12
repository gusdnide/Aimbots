using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Assault_Cube_Aimbot_2._0
{
    public partial class Form1 : Form
    {
        Process Processo;
        #region Estruturas
        class structJogador
        {
            public const int XOffset = 0x00;
            public const int YOffset = 0x04;
            public const int ZOffset = 0x08;
            public const int AngHOffset = 0x0C;
            public const int AngVOffset = 0x10;
            public const int SaudeOffset = 0xC4;
            //public const int TimeOffset = 0x32c; // Não vou usar por enquanto
            //public int Time = 0; // Não vou usar por enquanto
            public float PosX = 0f, PosY = 0f, PosZ = 0f; 
            public float AngloHo = 0f, AngloVer = 0f; 
            public int HP = 0;
        }
        struct PointerData
        {
            public int StructPTR;
            public int[] Offsets;

            public PointerData(int StructPTR, int[] Offsets)
            {
                this.StructPTR = StructPTR;
                this.Offsets = Offsets;
            }
        }
        #endregion
        #region Key
        [DllImport("user32.dll")]
        static extern ushort GetAsyncKeyState(int vKey);

        public static bool IsKeyPushedDown(System.Windows.Forms.Keys vKey)
        {
            return 0 != (GetAsyncKeyState((int)vKey) & 0x8000);
        }
        string keyBuffer = string.Empty;
        #endregion          
        #region Arrays
        PointerData JogPTR;
        List<PointerData> EnemyPTR = new List<PointerData>(); 

        IntPtr FinalJogPTR; 
        List<IntPtr> FinalEnemyPTR = new List<IntPtr>(); 

        structJogador JogData = new structJogador();
        List<structJogador> EnemyData = new List<structJogador>();
        const int JogadoresInGame = 31;
        #endregion
        public Form1()
        {
            InitializeComponent();
        }
        #region Buttons
        private void button1_Click(object sender, EventArgs e)
        {
            Process asda = Process.GetProcessesByName("ac_client")[0];
            Processo = asda;
            FinalJogPTR = (IntPtr)Memory.CalculatePointer(Processo, JogPTR.StructPTR, JogPTR.Offsets);
            FinalEnemyPTR.Clear();
            EnemyData.Clear();
            for (int index = 0; index < EnemyPTR.Count; index++)
            {
                FinalEnemyPTR.Add((IntPtr)Memory.CalculatePointer(Processo, EnemyPTR[index].StructPTR, EnemyPTR[index].Offsets));
                EnemyData.Add(new structJogador());
            }

            timer1.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
        }
        #endregion
        private void Form1_Load(object sender, EventArgs e)
        {
            JogPTR = new PointerData(0x00109B74, new int[] { 0x34 });
            for (int i = 1; i <= JogadoresInGame; i++)
                EnemyPTR.Add(new PointerData(0x0010F4F8, new int[] { 0x04 * i, 0x34 }));
        }
        

        private void timer1_Tick(object sender, EventArgs e)
        {

            #region Jogador
            JogData.PosX = Memory.ReadFloat(Processo, FinalJogPTR + structJogador.XOffset); 
            JogData.PosY = Memory.ReadFloat(Processo, FinalJogPTR + structJogador.YOffset);
            JogData.PosZ = Memory.ReadFloat(Processo, FinalJogPTR + structJogador.ZOffset);
            JogData.AngloHo = Memory.ReadFloat(Processo, FinalJogPTR + structJogador.AngHOffset);
            JogData.AngloVer = Memory.ReadFloat(Processo, FinalJogPTR + structJogador.AngVOffset);
            #endregion
            #region Jogadores
            for (int i = 0; i < EnemyData.Count; i++)
            {

                EnemyData[i].PosX = Memory.ReadFloat(Processo, FinalEnemyPTR[i] + structJogador.XOffset);
                EnemyData[i].PosY = Memory.ReadFloat(Processo, FinalEnemyPTR[i] + structJogador.YOffset);
                EnemyData[i].PosZ = Memory.ReadFloat(Processo, FinalEnemyPTR[i] + structJogador.ZOffset);
                EnemyData[i].HP = Memory.ReadInt32(Processo, FinalEnemyPTR[i] + structJogador.SaudeOffset);


            }
            #endregion
            #region Calcula o Proximo
            int InimigoProximo = -1; // Não tem ninguem perto 
            float InimigoDistancia = float.MaxValue; //Distancia do inimigo
            for (int i = 0; i < EnemyData.Count; i++)
            {
                if (EnemyData[i].HP <= 0)
                    continue;

                float DistanciaAtual = (float)Math.Sqrt(
                    (JogData.PosX - EnemyData[i].PosX) * (JogData.PosX - EnemyData[i].PosX) +
                    (JogData.PosY - EnemyData[i].PosY) * (JogData.PosY - EnemyData[i].PosY) +
                    (JogData.PosZ - EnemyData[i].PosZ) * (JogData.PosZ - EnemyData[i].PosZ));

                if (DistanciaAtual < InimigoDistancia)
                {
                    InimigoProximo = i;
                    InimigoDistancia = DistanciaAtual;

                }
            }
            #endregion
            #region Math.PI
            float RotacaoH = -(float)(Math.Atan2(EnemyData[InimigoProximo].PosX - JogData.PosX,
              EnemyData[InimigoProximo].PosY - JogData.PosY) / Math.PI * 180d) + 180f;
            float RotacaoV = (float)(Math.Atan2(
               EnemyData[InimigoProximo].PosZ - JogData.PosZ, Math.Sqrt(
                (EnemyData[InimigoProximo].PosX - JogData.PosX) * (EnemyData[InimigoProximo].PosX - JogData.PosX) +
                (EnemyData[InimigoProximo].PosY - JogData.PosY) * (EnemyData[InimigoProximo].PosY - JogData.PosY)))
                * 180.00 / Math.PI);
            
            #endregion
            #region Meche o Mouse
            if (IsKeyPushedDown(Keys.RButton) == true)
            {

                Memory.WriteFloat(Processo, FinalJogPTR + structJogador.AngHOffset, RotacaoH);
                Memory.WriteFloat(Processo, FinalJogPTR + structJogador.AngVOffset, RotacaoV);
              
            }
            #endregion
           
        }
    }
}
