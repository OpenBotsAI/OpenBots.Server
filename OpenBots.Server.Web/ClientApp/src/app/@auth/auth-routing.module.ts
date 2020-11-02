import { RouterModule, Routes } from '@angular/router';
import { NgModule } from '@angular/core';
import {
  NgxAuthComponent,
  NgxLoginComponent,
  NgxRegisterComponent,
  NgxRequestPasswordComponent,
  NgxResetPasswordComponent,
} from './components';
import { TermsConditionComponent } from './components/terms-condition/terms-condition.component';
import { ResetForgetPasswordComponent } from './components/reset-forget-password/reset-forget-password.component';
import { LoginGuard } from '../@core/guards/login.guard';
 
 

const routes: Routes = [{
  path: '',
  component: NgxAuthComponent,
  children: [
    {
      path: '',
      component: NgxLoginComponent
    },
    {
      path: 'login',
      component: NgxLoginComponent 
    },
    {
      path: 'register',
      component: NgxRegisterComponent,
    },
    {
      path: 'request-password',
      component: NgxRequestPasswordComponent,
    },
    {
      path: 'reset-password',
      component: NgxResetPasswordComponent,canActivate:[LoginGuard]
    },
    {
      path: 'terms-condition',
      component: TermsConditionComponent,canActivate:[LoginGuard]
    },
    {
      path: 'forgot-reset-password',
      component: ResetForgetPasswordComponent,
    },
  ],
}];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class AuthRoutingModule {
}
