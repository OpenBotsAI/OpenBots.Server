import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import {
  CIDR_IPv4_Pattern,
  CIDR_IPv6_Pattern,
  IPv4Regex,
  IPv6Regex,
} from '../../../../@auth/components';
import { HttpService } from '../../../../@core/services/http.service';
import { IpFencingApiUrl } from '../ipFencing';

@Component({
  selector: 'ngx-add-rule',
  templateUrl: './add-rule.component.html',
  styleUrls: ['./add-rule.component.scss'],
})
export class AddRuleComponent implements OnInit {
  usage = ['Allow', 'Deny'];
  rule = ['IPv4', 'IPv4Range', 'IPv6', 'IPv6Range', 'HTTP Header'];
  // usage: { name: string; value: number }[] = [
  //   { name: 'Allow', value: 1 },
  //   { name: 'Deny', value: -1 },
  // ];

  // rule: { name: string; value: number }[] = [
  //   { name: 'IPv4', value: 1 },
  //   // { name: 'IPv4Range ', value: 2 },
  //   { name: 'IPv6', value: 3 },
  //   // { name: 'IPv6Range ', value: 4 },
  //   { name: 'HTTP Header', value: 5 },
  // ];
  // rule = ['IPv4', 'IPv6', 'HTTP Header'];
  ruleForm: FormGroup;
  organizationId: string;
  constructor(
    private fb: FormBuilder,
    private httpService: HttpService,
    private router: Router
  ) {}

  ngOnInit(): void {
    if (localStorage.getItem('ActiveOrganizationID'))
      this.organizationId = localStorage.getItem('ActiveOrganizationID');
    this.ruleForm = this.initializeRuleForm();
  }

  initializeRuleForm() {
    return this.fb.group({
      usage: ['', [Validators.required]],
      rule: ['', [Validators.required]],
      ipAddress: [''],
      ipRange: [''],
      headerName: [''],
      headerValue: [''],
    });
  }

  get formControls() {
    return this.ruleForm.controls;
  }

  onSubmit(): void {
    this.httpService
      .post(
        `${IpFencingApiUrl.organizations}/${this.organizationId}/${IpFencingApiUrl.IPFencing}`,
        this.ruleForm.value,
        { observe: 'response' }
      )
      .subscribe((response) => {
        if (response && response.status === 201) {
          this.router.navigate(['/pages/config/settings']);
        }
      });
  }

  onRuleChange(event) {
    if (event.target.value == 'IPv4') {
      this.ruleForm.get('ipAddress').clearValidators();
      this.ruleForm
        .get('ipAddress')
        .setValidators([Validators.required, Validators.pattern(IPv4Regex)]);
      this.ruleForm.get('ipAddress').updateValueAndValidity();
    } else if (event.target.value == 'IPv6') {
      this.ruleForm.get('ipAddress').clearValidators();
      this.ruleForm
        .get('ipAddress')
        .setValidators([Validators.required, Validators.pattern(IPv6Regex)]);
      this.ruleForm.get('ipAddress').updateValueAndValidity();
    } else if (event.target.value == 'IPv4Range') {
      this.ruleForm.get('ipRange').clearValidators();
      this.ruleForm
        .get('ipRange')
        .setValidators([
          Validators.required,
          Validators.pattern(CIDR_IPv4_Pattern),
        ]);
      this.ruleForm.get('ipRange').updateValueAndValidity();
    } else if (event.target.value == 'IPv6Range') {
      this.ruleForm.get('ipRange').clearValidators();
      this.ruleForm
        .get('ipRange')
        .setValidators([
          Validators.required,
          Validators.pattern(CIDR_IPv6_Pattern),
        ]);
      this.ruleForm.get('ipRange').updateValueAndValidity();
    }
  }
}
