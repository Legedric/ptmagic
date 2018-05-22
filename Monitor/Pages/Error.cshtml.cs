using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Monitor.Pages {
  public class ErrorModel : PageModel {
    public string RequestId { get; set; }
    public IExceptionHandlerFeature Exception = null;

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    public void OnGet() {
      Exception = HttpContext.Features.Get<IExceptionHandlerFeature>();

      RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
    }
  }
}
