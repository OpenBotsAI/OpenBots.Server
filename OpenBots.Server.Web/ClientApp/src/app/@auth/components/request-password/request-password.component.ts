import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Inject, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { NB_AUTH_OPTIONS, NbAuthService } from '@nebular/auth';
import { getDeepFromObject } from '../../helpers';
import { EMAIL_PATTERN } from '../constants';
import { HttpService } from '../../../@core/services/http.service';
import { NbToastrService } from '@nebular/theme';

@Component({
  selector: 'ngx-request-password-page',
  styleUrls: ['./request-password.component.scss'],
  templateUrl: './request-password.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NgxRequestPasswordComponent implements OnInit {
  redirectDelay: number = this.getConfigValue('forms.requestPassword.redirectDelay');
  showMessages: any = this.getConfigValue('forms.requestPassword.showMessages');
  strategy: string = this.getConfigValue('forms.requestPassword.strategy');
  isEmailRequired: boolean = this.getConfigValue('forms.validation.email.required');

  submitted = false;
  errors: string[] = [];
  messages: string[] = [];
  user: any = {};
  requestPasswordForm: FormGroup;

  constructor(protected service: NbAuthService,
    @Inject(NB_AUTH_OPTIONS) protected options = {},
    protected cd: ChangeDetectorRef, private toastrService: NbToastrService,
    protected fb: FormBuilder, private httpService: HttpService,
    protected router: Router) { }

  get email() { return this.requestPasswordForm.get('email'); }

  ngOnInit(): void {
    const passwordValidators = [
      Validators.pattern(EMAIL_PATTERN),
    ];
    this.isEmailRequired && passwordValidators.push(Validators.required);

    this.requestPasswordForm = this.fb.group({
      email: this.fb.control('', [...passwordValidators]),
    });
  }

  requestPass(): void {
    this.user = this.requestPasswordForm.value;
    this.errors = this.messages = [];
    this.submitted = true;
    let formData = { email: this.requestPasswordForm.value.email, company: '' };
    this.httpService.post('Auth/ForgotPassword',formData).subscribe((result) => {
      this.toastrService.success('Please Check Your Email !','Success');
      this.router.navigate(['auth/login'])
    });
    this.submitted = false;
  }

  getConfigValue(key: string): any {
    return getDeepFromObject(this.options, key, null);
  }
}
