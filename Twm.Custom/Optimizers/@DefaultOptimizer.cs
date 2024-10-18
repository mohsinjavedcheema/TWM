using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twm.Core.DataCalc;
using Twm.Core.DataCalc.Optimization;

namespace Twm.Custom.Optimizers
{
    public class DefaultOptimizer:Optimizer
    {

        private const string OptimizerName = "Default Optimizer";

        private List<OptimizerParameter> _checkedParameters;

        public override async Task OnOptimize()
        {
            ResetParamValues();
            EnqueueParams();
            do
            {
               RunIteration();
                  

            } while (NextParamValues());
        }



        public void ResetParamValues()
        {
            _checkedParameters = OptimizerParameters.Where(x => x.IsChecked).ToList();
            foreach (var parameter in _checkedParameters)
            {
                parameter.ResetValue();
            }
        }


        public bool NextParamValues()
        {
            foreach (var parameter in _checkedParameters)
            {
                if (parameter.NextVal())
                {
                    EnqueueParams();
                    return true;
                }
                else
                {
                    parameter.ResetValue();
                }
            }

            return false;
        }

      


    }
}