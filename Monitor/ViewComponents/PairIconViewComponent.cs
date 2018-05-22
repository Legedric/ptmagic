using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Monitor.ViewComponents {
  public class PairIconViewComponent :ViewComponent  {
    public async Task<IViewComponentResult> InvokeAsync(Core.Main.DataObjects.PTMagicData.MarketPairSummary mps) {
      IViewComponentResult result = null;
      await Task.Run(() => {
        result = View(mps);
      });
      return result;
    }
  }
}
