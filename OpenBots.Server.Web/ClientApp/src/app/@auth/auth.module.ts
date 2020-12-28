import { NgModule, ModuleWithProviders, forwardRef } from '@angular/core';
import { NG_VALUE_ACCESSOR, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import {
  NbAuthModule,
} from '@nebular/auth';
import { RoleProvider } from './role.provider';
import { NbRoleProvider, NbSecurityModule } from '@nebular/security';
import { RecaptchaFormsModule, RecaptchaModule } from 'ng-recaptcha';

import {
  NgxLoginComponent,
  NgxAuthComponent,
  NgxAuthBlockComponent,
  NgxRegisterComponent,
  NgxRequestPasswordComponent,
  NgxResetPasswordComponent,
} from './components';

import {
  NbAlertModule,
  NbCardModule,
  NbIconModule,
  NbLayoutModule,
  NbCheckboxModule,
  NbInputModule,
  NbButtonModule,
} from '@nebular/theme';
import { AuthRoutingModule } from './auth-routing.module';
import { ComponentsModule } from '../@components/components.module';
import { TermsConditionComponent } from './components/terms-condition/terms-condition.component';
import { ResetForgetPasswordComponent } from './components/reset-forget-password/reset-forget-password.component';
import { LoginGuard } from '../@core/guards/login.guard';
import { TokenInterceptor } from '../@core/interceptor/token.interceptor';

const GUARDS = [LoginGuard];
const COMPONENTS = [
  NgxLoginComponent,
  NgxAuthComponent,
  NgxRegisterComponent,
  NgxRequestPasswordComponent,
  NgxResetPasswordComponent,
  NgxAuthBlockComponent,
  TermsConditionComponent,
  ResetForgetPasswordComponent
];

const NB_MODULES = [
  NbIconModule,
  NbLayoutModule,
  NbCardModule,
  NbAlertModule,
  NbCheckboxModule,
  NbInputModule,
  NbButtonModule,
  RecaptchaModule,
  RecaptchaFormsModule,
];

 

@NgModule({
  declarations: [...COMPONENTS],

  imports: [
    AuthRoutingModule,
    ReactiveFormsModule,
    CommonModule,
    ComponentsModule,
    ...NB_MODULES,
    NbAuthModule.forRoot(),
  ],
  exports: [],
  providers: [
    NbSecurityModule.forRoot().providers,
    {
      provide: NbRoleProvider,
      useClass: RoleProvider,
    },
    {
      provide: NG_VALUE_ACCESSOR,
      multi: true,
      useExisting: forwardRef(() => NgxRegisterComponent),
    },
  ],
})
export class AuthModule {
  static forRoot(): ModuleWithProviders<AuthModule> {
    return {
      ngModule: AuthModule,
      providers: [
        { provide: HTTP_INTERCEPTORS, useClass: TokenInterceptor, multi: true },
        ...GUARDS,
      ],
    };
  }
}
