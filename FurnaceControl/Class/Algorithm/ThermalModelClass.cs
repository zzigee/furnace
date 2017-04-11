﻿using System;
using System.Linq;

namespace FurnaceControl
{
    internal class ThermalModelClass : TimerClass
    {
        private readonly MainClass m_MainClass;

        float[] cp_ss400_Interpolation;
        float[] h_ss400_Interpolation;
        float[] f_ss400_Interpolation;

        float[] cp_sus304_Interpolation;
        float[] h_sus304_Interpolation;
        float[] f_sus304_Interpolation;

        public ThermalModelClass(MainClass mc, int timer_interval)
        {
            this.m_MainClass = mc;
            this.Start(timer_interval, "ThermalModelClassTimer");


            cp_ss400_Interpolation = InitInterpolationParameters(cp_ss400);
            h_ss400_Interpolation = InitInterpolationParameters(h_ss400);
            f_ss400_Interpolation = InitInterpolationParameters(f_ss400);


            cp_sus304_Interpolation = InitInterpolationParameters(cp_sus304);
            h_sus304_Interpolation = InitInterpolationParameters(h_sus304);
            f_sus304_Interpolation = InitInterpolationParameters(f_sus304);
        }

        float[,] cp_ss400 = new float[,] { { 0, 0.43f }, { 60, 0.443256f }, { 100, 0.454393f }, { 200, 0.482319f }, { 300, 0.506226f }, { 400, 0.524857f }, { 500, 0.546377f }, { 600, 0.573047f }, { 700, 0.604825f }, { 800, 0.633881f }, { 900, 0.689105f }, { 1000, 0.741356f }, { 1100, 0.792561f }, { 1200, 0.844435f }, { 1300, 0.89229f }, { 1350, 0.873492f }, { 1400, 0.75488f }, { 1500, 0.785904f }, { 1600, 0.680648f }, { 1700, 0.64506f }, { 1800, 0.653141f }, { 1900, 0.653141f }, { 2000, 0.653141f } };

        float[,] h_ss400 = new float[,] { { 0, 120f }, { 100, 120f }, { 200, 120f }, { 300, 120f }, { 301, 120f }, { 400, 120f }, { 500, 120f }, { 600, 120f }, { 601, 120f }, { 700, 120f }, { 800, 120f }, { 900, 120f }, { 901, 120f }, { 1000, 120f }, { 1100, 120f }, { 1200, 120f }, { 1201, 120f }, { 1300, 120f }, { 1400, 120f }, { 1500, 120f }, { 1600, 120f }, { 1700, 120f }, { 1800, 120f }, { 1900, 120f }, { 2000, 120f } };

        float[,] f_ss400 = new float[,] { { 0, 0.5f }, { 100, 0.5f }, { 200, 0.5f }, { 300, 0.5f }, { 301, 0.5f }, { 400, 0.5f }, { 500, 0.5f }, { 600, 0.5f }, { 601, 0.5f }, { 700, 0.5f }, { 800, 0.5f }, { 900, 0.5f }, { 901, 0.5f }, { 1000, 0.5f }, { 1100, 0.5f }, { 1200, 0.5f }, { 1201, 0.5f }, { 1300, 0.5f }, { 1400, 0.5f }, { 1500, 0.5f }, { 1600, 0.5f }, { 1700, 0.5f }, { 1800, 0.5f }, { 1900, 0.5f }, { 2000, 0.5f } };

        //비열 등 계수 상수 값 대입, 이후 DB에서 읽어올 내용 //테스트 용
        float[,] cp_sus304 = new float[,] { { 0, 0.41f }, { 86, 0.4339f }, { 392, 0.4947f }, { 752, 0.5298f }, { 1112, 0.5637f }, { 1292, 0.6020f }, { 1472, 0.6421f }, { 1652, 0.6562f }, { 1832, 0.6383f }, { 2012, 0.6582f } };

        float[,] h_sus304 = new float[,] { { 0, 80f }, { 100, 80f }, { 200, 80f }, { 300, 80f }, { 301, 80f }, { 400, 80f }, { 500, 80f }, { 600, 80f }, { 601, 80f }, { 700, 80f }, { 800, 80f }, { 900, 80f }, { 901, 80f }, { 1000, 80f }, { 1100, 80f }, { 1200, 80f }, { 1201, 80f }, { 1300, 80f }, { 1400, 80f }, { 1500, 80f }, { 1600, 80f }, { 1700, 80f }, { 1800, 80f }, { 1900, 80f }, { 2000, 80f } };

        float[,] f_sus304 = new float[,] { { 0, 0.5f }, { 100, 0.5f }, { 200, 0.5f }, { 300, 0.5f }, { 301, 0.5f }, { 400, 0.5f }, { 500, 0.5f }, { 600, 0.5f }, { 601, 0.5f }, { 700, 0.5f }, { 800, 0.5f }, { 900, 0.5f }, { 901, 0.5f }, { 1000, 0.5f }, { 1100, 0.5f }, { 1200, 0.5f }, { 1201, 0.5f }, { 1300, 0.5f }, { 1400, 0.5f }, { 1500, 0.5f }, { 1600, 0.5f }, { 1700, 0.5f }, { 1800, 0.5f }, { 1900, 0.5f }, { 2000, 0.5f } };



        private float[] InitInterpolationParameters(float[,] data)
        {
            int nArrayLength = data.Length / 2;                 // 2차 배열이므로 행의 갯수는 절반임 

            float[] x = new float[nArrayLength];
            float[] y = new float[nArrayLength];

            for (int i = 0; i < nArrayLength; i++)
            {
                x[i] = data[i, 0];
                y[i] = data[i, 1];
            }

            int range = (int)data[nArrayLength - 1, 0];         // 범위는 최대 온도 기준 
            float[] xs = new float[range];                      // 0에서 최대 온도까지의 정수단위별 파라메타 저장 (배열의 인덱스가 온도임) 
            float stepSize = (x[x.Length - 1] - x[0]) / (range - 1);

            for (int i = 0; i < range; i++)
            {
                xs[i] = x[0] + i * stepSize;
            }

            CubicSpline spline = new CubicSpline();
            float[] ys = spline.FitAndEval(x, y, xs);

            return ys;
        }

        /*********************************************************************************************
         *********************************************************************************************
         * 열모델 계산 코드 sus304
         *********************************************************************************************
         *********************************************************************************************/
        public void calThermalModel_sus304()
        {
            int idx = this.m_MainClass.m_Define_Class.nDataLoggingIndex;        // 현재 측정 인덱스 (최대치는 MAX_BILLET_IN_FURNACE  참조) 

            //********************************************************************************************
            // 시뮬레이션 모드에서만 사용 
            //********************************************************************************************
            //float fZoneTemprature = this.m_MainClass.stFURNACE_REALTIME_INFORMATION.fZone_Avg_Temperature[0];     // 현재 TC 온도 
            float fZoneTemprature = fn[idx, 1];     // 현재 TC 온도 

            double fCalBilletTemp;                  //예측 소재 온도
            float fPreBilletTemp;                   // 이전 빌렛 온도 

            int num_cp = 10;
            int num_h = 25;
            int num_f = 25;
            int num_fn = 601;
            int num_wp = 601;

            float dt; // unit sec 
            float dens, thick;
            float sigma, eps;
            float temp_wp_init;
            float cp_s, h_s, f_s;

            //상수 값 대입, 이후 DB에서 읽어올 내용
            dt = 60.0f;         // second 
            dens = 7915862f;
            thick = 0.0652f;
            sigma = 0.00000005669f;
            eps = 0.75f;
            //temp_wp_init = 50;
            cp_s = cp_sus304[0, 0];
            h_s = h_sus304[0, 0];
            f_s = f_sus304[0, 0];

            if (this.m_MainClass.m_Define_Class.nDataLoggingIndex == 0)
            {
                // 임의로 정의한 초기 빌렛 온도 
                fPreBilletTemp = 0.0f;
            }
            else
            {
                // 이전 계산된 빌렛 온도 
                fPreBilletTemp = this.m_MainClass.stBILLET_INFOMATION[this.m_MainClass.m_Define_Class.nDataLoggingIndex - 1].nBillet_Predict_Current_Billet_Temperature_304;
            }

            cp_s = cp_sus304_Interpolation[(int)fPreBilletTemp];
            h_s = h_sus304_Interpolation[(int)fPreBilletTemp];
            f_s = f_sus304_Interpolation[(int)fPreBilletTemp];

            double ffZone = (Math.Pow(fZoneTemprature + 273, 4.0));
            double ffBillet = (Math.Pow((fPreBilletTemp + 273), 4.0));

            // 열모델 지배 방정식                                           
            fCalBilletTemp = (fPreBilletTemp + 273) +
                (h_s * dt) *
                ((fZoneTemprature + 273) - (fPreBilletTemp + 273)) /
                (dens * cp_s * thick) +
                (sigma * eps * f_s * dt) *
                ((float)ffZone - (float)ffBillet) /
                (dens * cp_s * thick) - 273;


            this.m_MainClass.stBILLET_INFOMATION[idx].nZone_Average_Temperature = fZoneTemprature;
            this.m_MainClass.stBILLET_INFOMATION[idx].nBillet_Predict_Current_Billet_Temperature_304 = (float)fCalBilletTemp;
        }


        public void calThermalModel_ss400()
        {
            int idx = this.m_MainClass.m_Define_Class.nDataLoggingIndex;        // 현재 측정 인덱스 (최대치는 MAX_BILLET_IN_FURNACE  참조) 

            //********************************************************************************************
            // 시뮬레이션 모드에서만 사용 
            //********************************************************************************************
            //float fZoneTemprature = this.m_MainClass.stFURNACE_REALTIME_INFORMATION.fZone_Avg_Temperature[0];     // 현재 TC 온도 
            float fZoneTemprature = fn[idx, 1];     // 현재 TC 온도 

            double fCalBilletTemp;                  //예측 소재 온도
            float fPreBilletTemp;                   // 이전 빌렛 온도 

            int num_cp = 23;
            int num_h = 25;
            int num_f = 25;
            int num_fn = 601;
            int num_wp = 601;

            float dt; // unit sec 
            float dens, thick;
            float sigma, eps;
            float cp_s, h_s, f_s;

            //상수 값 대입, 이후 DB에서 읽어올 내용
            dt = 60.0f;         // second 
            dens = 7995000f;
            thick = 0.0652f;
            sigma = 0.00000005669f;
            eps = 0.7f;
            //temp_wp_init = 50;
            cp_s = cp_ss400[0, 0];
            h_s = h_ss400[0, 0];
            f_s = f_ss400[0, 0];

            if (this.m_MainClass.m_Define_Class.nDataLoggingIndex == 0)
            {
                // 임의로 정의한 초기 빌렛 온도
                fPreBilletTemp = 0.0f;
            }
            else
            {
                // 이전 계산된 빌렛 온도 
                fPreBilletTemp = this.m_MainClass.stBILLET_INFOMATION[this.m_MainClass.m_Define_Class.nDataLoggingIndex - 1].nBillet_Predict_Current_Billet_Temperature_400;
            }

            cp_s = cp_ss400_Interpolation[(int)fPreBilletTemp];
            h_s = h_ss400_Interpolation[(int)fPreBilletTemp];
            f_s = f_ss400_Interpolation[(int)fPreBilletTemp];

            double ffZone = (Math.Pow(fZoneTemprature + 273, 4.0));     // 
            double ffBillet = (Math.Pow((fPreBilletTemp + 273), 4.0)); // 

            // 열모델 지배 방정식                                           
            fCalBilletTemp = (fPreBilletTemp + 273) +
                (h_s * dt) *
                ((fZoneTemprature + 273) - (fPreBilletTemp + 273)) /
                (dens * cp_s * thick) +
                (sigma * eps * f_s * dt) *
                ((float)ffZone - (float)ffBillet) /
                (dens * cp_s * thick) - 273;

            this.m_MainClass.stBILLET_INFOMATION[idx].nZone_Average_Temperature = fZoneTemprature;
            this.m_MainClass.stBILLET_INFOMATION[idx].nBillet_Predict_Current_Billet_Temperature_400 = (float)fCalBilletTemp;

        }

        public override void Run()
        {
            this.m_MainClass.m_SysLogClass.DebugLog(this, "ThermalModelClassTimer");

            /*
            Start Data Logging 이 활성화 된 경우 실행 
            실행 주기는 열모델 연산 Delta 시간과 동일하게 DB_Timer 에 설정하면 됨. 
            */
            if (this.m_MainClass.m_Define_Class.isDataLogging)
            {

                /**
                 * 열모델 기반 소재온도 계산  
                 */
                calThermalModel_sus304();
                calThermalModel_ss400();



                /**
                 * DB 저장 
                 */
                this.m_MainClass.m_MainForm.dANGJIN_DATATableAdapter.InsertQuery(
                    this.m_MainClass.m_Define_Class.nDataLoggingIndex,
                    DateTime.Now.ToString(),
                    this.m_MainClass.stFURNACE_REALTIME_INFORMATION.fZone_Avg_Temperature[0],
                    this.m_MainClass.stFURNACE_REALTIME_INFORMATION.fZone_Temperature[0],
                    this.m_MainClass.stFURNACE_REALTIME_INFORMATION.fZone_Temperature[1],
                    this.m_MainClass.stFURNACE_REALTIME_INFORMATION.fZone_Temperature[2],
                    this.m_MainClass.stFURNACE_REALTIME_INFORMATION.fZone_Temperature[3],
                    this.m_MainClass.stFURNACE_REALTIME_INFORMATION.fZone_Temperature[4],
                    this.m_MainClass.stFURNACE_REALTIME_INFORMATION.fZone_Temperature[5],
                    this.m_MainClass.stFURNACE_REALTIME_INFORMATION.fZone_Temperature[6],
                    this.m_MainClass.stFURNACE_REALTIME_INFORMATION.fZone_Temperature[7],
                    this.m_MainClass.stFURNACE_REALTIME_INFORMATION.fZone_Temperature[8],
                    this.m_MainClass.stFURNACE_REALTIME_INFORMATION.fZone_Temperature[9],
                    this.m_MainClass.stFURNACE_REALTIME_INFORMATION.fZone_Temperature[10],
                    this.m_MainClass.stFURNACE_REALTIME_INFORMATION.fZone_Temperature[11],
                    this.m_MainClass.stBILLET_INFOMATION[this.m_MainClass.m_Define_Class.nDataLoggingIndex].nBillet_Predict_Current_Billet_Temperature_304,
                    this.m_MainClass.stBILLET_INFOMATION[this.m_MainClass.m_Define_Class.nDataLoggingIndex].nBillet_Predict_Current_Billet_Temperature_400);
                
                /**
                 * 데이터 수집 인덱스 증가 
                 */
                this.m_MainClass.m_Define_Class.nDataLoggingIndex++;    // 열모델 배열 증가

                this.m_MainClass.m_MainForm.Set_txtDanjin_Current_Date(DateTime.Now.ToString());

                TimeSpan result = DateTime.Now - this.m_MainClass.m_Define_Class.dateDataLoggingStartTime;
                this.m_MainClass.m_MainForm.Set_txtDanjin_Operation_Time("[" + this.m_MainClass.m_Define_Class.nDataLoggingIndex + "]" + result.ToString(@"h\:mm\:ss"));

                
                /**
                 * 테스트 종료 확인 
                 */
                if (this.m_MainClass.m_Define_Class.nDataLoggingIndex >= this.m_MainClass.m_Define_Class.MAX_BILLET_IN_FURNACE)
                {
                    this.m_MainClass.m_MainForm.ShowMessageBox("당진 테스트베드 측정 데이터 갯수가 최대 갯수를 초과하였습니다. \n\r측정을 중지합니다.");
                    this.m_MainClass.m_MainForm.btnDataLogging.BackColor = System.Drawing.Color.LightGray;
                    this.m_MainClass.m_Define_Class.isDataLogging = false;
                }
            }
        }


        double getLagrange(double[] x, double[] y, int n, double t)
        {
            int i, j;
            double s, p;

            s = 0.0;

            for (i = 0; i < n; i++)
            {
                p = y[i];
                for (j = 0; j < n; j++)
                {
                    if (i != j)
                    {
                        p = p * (t - x[j]) / (x[i] - x[j]);
                    }
                }
                s = s + p;
            }
            return s;
        }


        float getLagrange_new(float[,] data, int n, float t)
        {
            int i, j;
            float s, p;

            s = 0.0f;

            for (i = 0; i < n; i++)
            {
                p = data[i, 1];
                for (j = 0; j < n; j++)
                {
                    if (i != j)
                    {
                        p = p * (t - data[j, 0]) / (data[i, 0] - data[j, 0]);
                    }
                }
                s = s + p;
            }
            return s;
        }













        float[,] fn = new float[,] { { 0, 20f }, { 60, 28f }, { 120, 36f }, { 180, 44f }, { 240, 52f }, { 300, 60f }, { 360, 68f }, { 420, 76f }, { 480, 84f }, { 540, 92f }, { 600, 100f }, { 660, 108f }, { 720, 116f }, { 780, 124f }, { 840, 132f }, { 900, 140f }, { 960, 148f }, { 1020, 156f }, { 1080, 164f }, { 1140, 172f }, { 1200, 180f }, { 1260, 188f }, { 1320, 196f }, { 1380, 204f }, { 1440, 212f }, { 1500, 220f }, { 1560, 228f }, { 1620, 236f }, { 1680, 244f }, { 1740, 252f }, { 1800, 260f }, { 1860, 268f }, { 1920, 276f }, { 1980, 284f }, { 2040, 292f }, { 2100, 300f }, { 2160, 308f }, { 2220, 316f }, { 2280, 324f }, { 2340, 332f }, { 2400, 340f }, { 2460, 348f }, { 2520, 356f }, { 2580, 364f }, { 2640, 372f }, { 2700, 380f }, { 2760, 388f }, { 2820, 396f }, { 2880, 404f }, { 2940, 412f }, { 3000, 420f }, { 3060, 428f }, { 3120, 436f }, { 3180, 444f }, { 3240, 452f }, { 3300, 460f }, { 3360, 468f }, { 3420, 476f }, { 3480, 484f }, { 3540, 492f }, { 3600, 500f }, { 3660, 500f }, { 3720, 500f }, { 3780, 500f }, { 3840, 500f }, { 3900, 500f }, { 3960, 500f }, { 4020, 500f }, { 4080, 500f }, { 4140, 500f }, { 4200, 500f }, { 4260, 500f }, { 4320, 500f }, { 4380, 500f }, { 4440, 500f }, { 4500, 500f }, { 4560, 500f }, { 4620, 500f }, { 4680, 500f }, { 4740, 500f }, { 4800, 500f }, { 4860, 500f }, { 4920, 500f }, { 4980, 500f }, { 5040, 500f }, { 5100, 500f }, { 5160, 500f }, { 5220, 500f }, { 5280, 500f }, { 5340, 500f }, { 5400, 500f }, { 5460, 505f }, { 5520, 510f }, { 5580, 515f }, { 5640, 520f }, { 5700, 525f }, { 5760, 530f }, { 5820, 535f }, { 5880, 540f }, { 5940, 545f }, { 6000, 550f }, { 6060, 555f }, { 6120, 560f }, { 6180, 565f }, { 6240, 570f }, { 6300, 575f }, { 6360, 580f }, { 6420, 585f }, { 6480, 590f }, { 6540, 595f }, { 6600, 600f }, { 6660, 605f }, { 6720, 610f }, { 6780, 615f }, { 6840, 620f }, { 6900, 625f }, { 6960, 630f }, { 7020, 635f }, { 7080, 640f }, { 7140, 645f }, { 7200, 650f }, { 7260, 655f }, { 7320, 660f }, { 7380, 665f }, { 7440, 670f }, { 7500, 675f }, { 7560, 680f }, { 7620, 685f }, { 7680, 690f }, { 7740, 695f }, { 7800, 700f }, { 7860, 705f }, { 7920, 710f }, { 7980, 715f }, { 8040, 720f }, { 8100, 725f }, { 8160, 730f }, { 8220, 735f }, { 8280, 740f }, { 8340, 745f }, { 8400, 750f }, { 8460, 755f }, { 8520, 760f }, { 8580, 765f }, { 8640, 770f }, { 8700, 775f }, { 8760, 780f }, { 8820, 785f }, { 8880, 790f }, { 8940, 795f }, { 9000, 800f }, { 9060, 800f }, { 9120, 800f }, { 9180, 800f }, { 9240, 800f }, { 9300, 800f }, { 9360, 800f }, { 9420, 800f }, { 9480, 800f }, { 9540, 800f }, { 9600, 800f }, { 9660, 800f }, { 9720, 800f }, { 9780, 800f }, { 9840, 800f }, { 9900, 800f }, { 9960, 800f }, { 10020, 800f }, { 10080, 800f }, { 10140, 800f }, { 10200, 800f }, { 10260, 800f }, { 10320, 800f }, { 10380, 800f }, { 10440, 800f }, { 10500, 800f }, { 10560, 800f }, { 10620, 800f }, { 10680, 800f }, { 10740, 800f }, { 10800, 800f }, { 10860, 806f }, { 10920, 813f }, { 10980, 820f }, { 11040, 826f }, { 11100, 833f }, { 11160, 840f }, { 11220, 846f }, { 11280, 853f }, { 11340, 860f }, { 11400, 866f }, { 11460, 873f }, { 11520, 880f }, { 11580, 886f }, { 11640, 893f }, { 11700, 900f }, { 11760, 906f }, { 11820, 913f }, { 11880, 920f }, { 11940, 926f }, { 12000, 933f }, { 12060, 940f }, { 12120, 946f }, { 12180, 953f }, { 12240, 960f }, { 12300, 966f }, { 12360, 973f }, { 12420, 980f }, { 12480, 986f }, { 12540, 993f }, { 12600, 1000f }, { 12660, 1003f }, { 12720, 1006f }, { 12780, 1009f }, { 12840, 1013f }, { 12900, 1016f }, { 12960, 1019f }, { 13020, 1023f }, { 13080, 1026f }, { 13140, 1029f }, { 13200, 1033f }, { 13260, 1036f }, { 13320, 1039f }, { 13380, 1043f }, { 13440, 1046f }, { 13500, 1049f }, { 13560, 1053f }, { 13620, 1056f }, { 13680, 1059f }, { 13740, 1063f }, { 13800, 1066f }, { 13860, 1069f }, { 13920, 1073f }, { 13980, 1076f }, { 14040, 1079f }, { 14100, 1083f }, { 14160, 1086f }, { 14220, 1089f }, { 14280, 1093f }, { 14340, 1096f }, { 14400, 1100f }, { 14460, 1102f }, { 14520, 1105f }, { 14580, 1108f }, { 14640, 1110f }, { 14700, 1113f }, { 14760, 1116f }, { 14820, 1118f }, { 14880, 1121f }, { 14940, 1124f }, { 15000, 1126f }, { 15060, 1129f }, { 15120, 1132f }, { 15180, 1134f }, { 15240, 1137f }, { 15300, 1140f }, { 15360, 1142f }, { 15420, 1145f }, { 15480, 1148f }, { 15540, 1150f }, { 15600, 1153f }, { 15660, 1156f }, { 15720, 1158f }, { 15780, 1161f }, { 15840, 1164f }, { 15900, 1166f }, { 15960, 1169f }, { 16020, 1172f }, { 16080, 1174f }, { 16140, 1177f }, { 16200, 1180f }, { 16260, 1182f }, { 16320, 1184f }, { 16380, 1186f }, { 16440, 1189f }, { 16500, 1191f }, { 16560, 1193f }, { 16620, 1196f }, { 16680, 1198f }, { 16740, 1200f }, { 16800, 1203f }, { 16860, 1205f }, { 16920, 1207f }, { 16980, 1210f }, { 17040, 1212f }, { 17100, 1214f }, { 17160, 1217f }, { 17220, 1219f }, { 17280, 1221f }, { 17340, 1224f }, { 17400, 1226f }, { 17460, 1228f }, { 17520, 1231f }, { 17580, 1233f }, { 17640, 1235f }, { 17700, 1238f }, { 17760, 1240f }, { 17820, 1242f }, { 17880, 1245f }, { 17940, 1247f }, { 18000, 1250f }, { 18060, 1250f }, { 18120, 1250f }, { 18180, 1250f }, { 18240, 1250f }, { 18300, 1250f }, { 18360, 1250f }, { 18420, 1250f }, { 18480, 1250f }, { 18540, 1250f }, { 18600, 1250f }, { 18660, 1250f }, { 18720, 1250f }, { 18780, 1250f }, { 18840, 1250f }, { 18900, 1250f }, { 18960, 1250f }, { 19020, 1250f }, { 19080, 1250f }, { 19140, 1250f }, { 19200, 1250f }, { 19260, 1250f }, { 19320, 1250f }, { 19380, 1250f }, { 19440, 1250f }, { 19500, 1250f }, { 19560, 1250f }, { 19620, 1250f }, { 19680, 1250f }, { 19740, 1250f }, { 19800, 1250f }, { 19860, 1250f }, { 19920, 1250f }, { 19980, 1250f }, { 20040, 1250f }, { 20100, 1250f }, { 20160, 1250f }, { 20220, 1250f }, { 20280, 1250f }, { 20340, 1250f }, { 20400, 1250f }, { 20460, 1250f }, { 20520, 1250f }, { 20580, 1250f }, { 20640, 1250f }, { 20700, 1250f }, { 20760, 1250f }, { 20820, 1250f }, { 20880, 1250f }, { 20940, 1250f }, { 21000, 1250f }, { 21060, 1250f }, { 21120, 1250f }, { 21180, 1250f }, { 21240, 1250f }, { 21300, 1250f }, { 21360, 1250f }, { 21420, 1250f }, { 21480, 1250f }, { 21540, 1250f }, { 21600, 1250f }, { 21660, 1250f }, { 21720, 1250f }, { 21780, 1250f }, { 21840, 1250f }, { 21900, 1250f }, { 21960, 1250f }, { 22020, 1250f }, { 22080, 1250f }, { 22140, 1250f }, { 22200, 1250f }, { 22260, 1250f }, { 22320, 1250f }, { 22380, 1250f }, { 22440, 1250f }, { 22500, 1250f }, { 22560, 1250f }, { 22620, 1250f }, { 22680, 1250f }, { 22740, 1250f }, { 22800, 1250f }, { 22860, 1250f }, { 22920, 1250f }, { 22980, 1250f }, { 23040, 1250f }, { 23100, 1250f }, { 23160, 1250f }, { 23220, 1250f }, { 23280, 1250f }, { 23340, 1250f }, { 23400, 1250f }, { 23460, 1250f }, { 23520, 1250f }, { 23580, 1250f }, { 23640, 1250f }, { 23700, 1250f }, { 23760, 1250f }, { 23820, 1250f }, { 23880, 1250f }, { 23940, 1250f }, { 24000, 1250f }, { 24060, 1250f }, { 24120, 1250f }, { 24180, 1250f }, { 24240, 1250f }, { 24300, 1250f }, { 24360, 1250f }, { 24420, 1250f }, { 24480, 1250f }, { 24540, 1250f }, { 24600, 1250f }, { 24660, 1250f }, { 24720, 1250f }, { 24780, 1250f }, { 24840, 1250f }, { 24900, 1250f }, { 24960, 1250f }, { 25020, 1250f }, { 25080, 1250f }, { 25140, 1250f }, { 25200, 1250f }, { 25260, 1250f }, { 25320, 1250f }, { 25380, 1250f }, { 25440, 1250f }, { 25500, 1250f }, { 25560, 1250f }, { 25620, 1250f }, { 25680, 1250f }, { 25740, 1250f }, { 25800, 1250f }, { 25860, 1250f }, { 25920, 1250f }, { 25980, 1250f }, { 26040, 1250f }, { 26100, 1250f }, { 26160, 1250f }, { 26220, 1250f }, { 26280, 1250f }, { 26340, 1250f }, { 26400, 1250f }, { 26460, 1250f }, { 26520, 1250f }, { 26580, 1250f }, { 26640, 1250f }, { 26700, 1250f }, { 26760, 1250f }, { 26820, 1250f }, { 26880, 1250f }, { 26940, 1250f }, { 27000, 1250f }, { 27060, 1250f }, { 27120, 1250f }, { 27180, 1250f }, { 27240, 1250f }, { 27300, 1250f }, { 27360, 1250f }, { 27420, 1250f }, { 27480, 1250f }, { 27540, 1250f }, { 27600, 1250f }, { 27660, 1250f }, { 27720, 1250f }, { 27780, 1250f }, { 27840, 1250f }, { 27900, 1250f }, { 27960, 1250f }, { 28020, 1250f }, { 28080, 1250f }, { 28140, 1250f }, { 28200, 1250f }, { 28260, 1250f }, { 28320, 1250f }, { 28380, 1250f }, { 28440, 1250f }, { 28500, 1250f }, { 28560, 1250f }, { 28620, 1250f }, { 28680, 1250f }, { 28740, 1250f }, { 28800, 1250f }, { 28860, 1250f }, { 28920, 1250f }, { 28980, 1250f }, { 29040, 1250f }, { 29100, 1250f }, { 29160, 1250f }, { 29220, 1250f }, { 29280, 1250f }, { 29340, 1250f }, { 29400, 1250f }, { 29460, 1250f }, { 29520, 1250f }, { 29580, 1250f }, { 29640, 1250f }, { 29700, 1250f }, { 29760, 1250f }, { 29820, 1250f }, { 29880, 1250f }, { 29940, 1250f }, { 30000, 1250f }, { 30060, 1250f }, { 30120, 1250f }, { 30180, 1250f }, { 30240, 1250f }, { 30300, 1250f }, { 30360, 1250f }, { 30420, 1250f }, { 30480, 1250f }, { 30540, 1250f }, { 30600, 1250f }, { 30660, 1250f }, { 30720, 1250f }, { 30780, 1250f }, { 30840, 1250f }, { 30900, 1250f }, { 30960, 1250f }, { 31020, 1250f }, { 31080, 1250f }, { 31140, 1250f }, { 31200, 1250f }, { 31260, 1250f }, { 31320, 1250f }, { 31380, 1250f }, { 31440, 1250f }, { 31500, 1250f }, { 31560, 1250f }, { 31620, 1250f }, { 31680, 1250f }, { 31740, 1250f }, { 31800, 1250f }, { 31860, 1250f }, { 31920, 1250f }, { 31980, 1250f }, { 32040, 1250f }, { 32100, 1250f }, { 32160, 1250f }, { 32220, 1250f }, { 32280, 1250f }, { 32340, 1250f }, { 32400, 1250f }, { 32460, 1250f }, { 32520, 1250f }, { 32580, 1250f }, { 32640, 1250f }, { 32700, 1250f }, { 32760, 1250f }, { 32820, 1250f }, { 32880, 1250f }, { 32940, 1250f }, { 33000, 1250f }, { 33060, 1250f }, { 33120, 1250f }, { 33180, 1250f }, { 33240, 1250f }, { 33300, 1250f }, { 33360, 1250f }, { 33420, 1250f }, { 33480, 1250f }, { 33540, 1250f }, { 33600, 1250f }, { 33660, 1250f }, { 33720, 1250f }, { 33780, 1250f }, { 33840, 1250f }, { 33900, 1250f }, { 33960, 1250f }, { 34020, 1250f }, { 34080, 1250f }, { 34140, 1250f }, { 34200, 1250f }, { 34260, 1250f }, { 34320, 1250f }, { 34380, 1250f }, { 34440, 1250f }, { 34500, 1250f }, { 34560, 1250f }, { 34620, 1250f }, { 34680, 1250f }, { 34740, 1250f }, { 34800, 1250f }, { 34860, 1250f }, { 34920, 1250f }, { 34980, 1250f }, { 35040, 1250f }, { 35100, 1250f }, { 35160, 1250f }, { 35220, 1250f }, { 35280, 1250f }, { 35340, 1250f }, { 35400, 1250f }, { 35460, 1250f }, { 35520, 1250f }, { 35580, 1250f }, { 35640, 1250f }, { 35700, 1250f }, { 35760, 1250f }, { 35820, 1250f }, { 35880, 1250f }, { 35940, 1250f }, { 36000, 1250f } };
    }
}