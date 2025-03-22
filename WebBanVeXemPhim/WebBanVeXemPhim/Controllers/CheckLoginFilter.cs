using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class CheckLoginFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var session = context.HttpContext.Session;
        int? nguoiDung = session.GetInt32("NguoiDung");

        if (nguoiDung == null || nguoiDung == 0)
        {
            context.Result = new RedirectToActionResult("Index", "Login", null);
        }

        base.OnActionExecuting(context);
    }
}
