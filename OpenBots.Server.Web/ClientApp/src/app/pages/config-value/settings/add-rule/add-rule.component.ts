import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HttpService } from '../../../../@core/services/http.service';
import { IpFencingApiUrl } from '../ipFencing';

@Component({
  selector: 'ngx-add-rule',
  templateUrl: './add-rule.component.html',
  styleUrls: ['./add-rule.component.scss'],
})
export class AddRuleComponent implements OnInit {
  usage = ['Allow', 'Deny'];
  rule = [
    'IPv4',
    //'IPv4Range',
    'IPv6',
    //'IPv6Range',
    'HTTP Header',
  ];
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
  constructor(private fb: FormBuilder, private httpService: HttpService) {}

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
    // this.ruleForm.value.usage = +this.ruleForm.value.usage;
    // this.ruleForm.value.rule = +this.ruleForm.value.rule;
    // this.ruleForm.value.usage = Number(this.ruleForm.value.usage);
    console.log('submit', this.ruleForm.value);

    this.httpService
      .post(
        `${IpFencingApiUrl.organizations}/${this.organizationId}/${IpFencingApiUrl.IPFencing}`,
        this.ruleForm.value
      )
      .subscribe((response) => console.log('res', response));
  }
}
