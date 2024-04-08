import { AuthService } from 'src/app/services/auth.service';
import { ActivatedRouteSnapshot, CanActivateChildFn, CanActivateFn, Router, RouterStateSnapshot } from '@angular/router';
import { Inject, inject } from '@angular/core';
import { NgToastService } from 'ng-angular-popup';


export const authGuard:CanActivateFn  = (
  route , state ,) => {
    const router = inject(Router);
    let Islogin = localStorage.getItem('token');
    const auth = inject(AuthService);
    const ngToast =  inject(NgToastService);
    if (auth.isLoffedIn()) {
     
      return true;
    }else 
    {
      ngToast.error({detail: "ERROR" , summary:"Please Login First"});
      router.navigate(['login']);
      return false;
    }
 
};
