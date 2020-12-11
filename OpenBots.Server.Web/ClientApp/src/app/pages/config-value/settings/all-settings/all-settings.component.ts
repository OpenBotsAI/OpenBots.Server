import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpService } from '../../../../@core/services/http.service';
import { IPFencing } from '../../../../interfaces/ipFencing';
import { IpFencingApiUrl } from '../ipFencing';

@Component({
  selector: 'ngx-all-settings',
  templateUrl: './all-settings.component.html',
  styleUrls: ['./all-settings.component.scss'],
})
export class AllSettingsComponent implements OnInit {
  organizationId: string;
  orgSettingId: string;
  IPFencingData: IPFencing[] = [];
  isChecked: boolean;
  ipFencingForm: FormGroup;
  usage: { name: string; value: number }[] = [
    { name: 'Allow', value: 1 },
    { name: 'Deny', value: -1 },
  ];

  rule: { name: string; value: number }[] = [
    { name: 'IPv4', value: 1 },
    { name: 'IPv4Range ', value: 2 },
    { name: 'IPv6', value: 3 },
    { name: 'IPv6Range ', value: 4 },
    { name: 'HTTP Header', value: 5 },
  ];
  constructor(
    private router: Router,
    private httpService: HttpService,
    private fb: FormBuilder
  ) {}

  ngOnInit(): void {
    if (localStorage.getItem('ActiveOrganizationID'))
      this.organizationId = localStorage.getItem('ActiveOrganizationID');
    this.getSettingsId();
    this.getAllIPFencing();
    this.ipFencingForm = this.fb.group({
      ipFencingMode: [],
    });
  }

  getSettingsId(): void {
    this.httpService
      .get(
        `${IpFencingApiUrl.organizations}/${this.organizationId}/${IpFencingApiUrl.organizationSettings}`
      )
      .subscribe((response) => {
        if (response && response.items.length)
          this.orgSettingId = response.items[0].id;
        this.getToggleButtonState();
      });
  }

  getAllIPFencing(): void {
    this.httpService
      .get(
        `${IpFencingApiUrl.organizations}/${this.organizationId}/${IpFencingApiUrl.IPFencing}`
      )
      .subscribe((response) => {
        if (response && response.items.length) {
          for (let items of response.items) {
            for (let data of this.usage) {
              if (items.usage == data.value) {
                items.usage = data.name;
              }
            }
            for (let data of this.rule) {
              if (items.rule == data.value) {
                items.rule = data.name;
              }
            }
          }
          this.IPFencingData = response.items;
        }
      });
  }
  addRule(): void {
    this.router.navigate(['pages/config/settings/rule/add']);
  }

  onToggleSecurityModel(event): void {
    this.isChecked = event.target.checked;
    let data: number;
    if (event.target.checked) data = 1;
    else data = -1;
    console.log('toggle', event.target.checked);
    const arr = [
      {
        value: data,
        path: '/ipFencingMode',
        op: 'replace',
      },
    ];
    this.httpService
      .patch(
        `${IpFencingApiUrl.organizations}/${this.organizationId}/${IpFencingApiUrl.organizationSettings}/${this.orgSettingId}`,
        arr,
        { observe: 'response' }
      )
      .subscribe((response) => {
        console.log('patch', response);
      });
  }

  getToggleButtonState(): void {
    this.httpService
      .get(
        `${IpFencingApiUrl.organizations}/${this.organizationId}/${IpFencingApiUrl.organizationSettings}/${this.orgSettingId}`
      )
      .subscribe((response) => {
        if (response && response.ipFencingMode) {
          if (response.ipFencingMode == 1) {
            this.isChecked = true;
            this.ipFencingForm.patchValue({
              ipFencingMode: true,
            });
          } else {
            this.isChecked = false;
            this.ipFencingForm.patchValue({
              ipFencingMode: false,
            });
          }
        }
      });
  }
}
