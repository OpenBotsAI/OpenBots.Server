import { Component, OnInit } from '@angular/core';
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
  IPFencingData: IPFencing[] = [];
  isChecked: boolean;
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
  constructor(private router: Router, private httpService: HttpService) {}

  ngOnInit(): void {
    if (localStorage.getItem('ActiveOrganizationID'))
      this.organizationId = localStorage.getItem('ActiveOrganizationID');
    this.getAllIPFencing();
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
    console.log('toggle', event.target.checked);
    const arr = [
      {
        value: event.target.checked,
        path: 'ipFencingMode',
        op: 'replace',
      },
    ];
    this.httpService
      .patch(
        `${IpFencingApiUrl.organizationSettings}/${this.organizationId}`,
        arr
      )
      .subscribe((res) => {
        console.log('res', res);
      });
  }
}
