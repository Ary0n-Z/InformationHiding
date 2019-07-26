using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIH_core.ViewModels
{
    class StatisticsViewModel
    {

            public double SNR
            {
                get; set;
            }
            public int AM { get; set; }
            public double Mean { get; set; }
            public double SD { get; set; }
            public double MSE { get; set; }
        public StatisticsViewModel(int[] signal,int[] stego)
        {
            int[] error = new int[signal.Length];
            for(int i = 0; i < signal.Length; i++)
                error[i] = signal[i] - stego[i];
            int max = 0;
            double avarage = 0;
            int sumOfSquareError = 0;
            int sumOfSquareSignal = 0;
            for (int i =0; i< error.Length; i++)
            {
                //AM
                if (Math.Abs(error[i]) > max) max = error[i];
                avarage += error[i];
                sumOfSquareError += error[i] * error[i];
                sumOfSquareSignal += signal[i] * signal[i];
            }
            Mean = avarage / error.Length;
            AM = max;
            MSE = avarage;
            SNR = 10 * Math.Log10(sumOfSquareSignal / (double)sumOfSquareError);
            double sd = 0;
            for (int i = 0; i < error.Length; i++)
                sd += error[i] - Mean;
            SD = Math.Sqrt(sd /error.Length);
        }
    }
}
