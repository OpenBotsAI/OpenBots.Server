import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { HttpService } from '../../../@core/services/http.service';
import { AgentGroup } from '../../../interfaces/agentGroup';
import { AgentApiUrl, AgentGroupAPiUrl } from '../../../webApiUrls';

@Component({
  selector: 'ngx-view-agent-group',
  templateUrl: './view-agent-group.component.html',
  styleUrls: ['./view-agent-group.component.scss'],
})
export class ViewAgentGroupComponent implements OnInit {
  urlId: string;
  agentGroupForm: FormGroup;
  dropdownSettings = {};
  allAgentsArr: AgentGroup[];
  constructor(
    private route: ActivatedRoute,
    private httpService: HttpService,
    private fb: FormBuilder
  ) {}

  ngOnInit(): void {
    this.urlId = this.route.snapshot.params['id'];
    this.getAllAgents();
    if (this.urlId) {
      this.getAgentGroupById();
    }
    this.agentGroupForm = this.initializeAgentGroupForm();
    this.multiDropDownSettings();
  }

  initializeAgentGroupForm() {
    return this.fb.group({
      name: [''],
      isEnabled: [''],
      agentGroupMembers: [],
      description: [''],
    });
  }

  multiDropDownSettings(): void {
    this.dropdownSettings = {
      text: 'Please select multiple agents',
      selectAllText: 'Select All',
      unSelectAllText: 'UnSelect All',
      enableSearchFilter: true,
      classes: 'myclass custom-class',
      maxHeight: 250,
      badgeShowLimit: 5,
      searchBy: ['agentName'],
      searchAutofocus: true,
      primaryKey: 'agentId',
      labelKey: 'agentName',
      disabled: true,
    };
  }
  getAgentGroupById(): void {
    this.httpService
      .get(
        `${AgentGroupAPiUrl.agentGroups}/${AgentGroupAPiUrl.view}/${this.urlId}`
      )
      .subscribe((response) => {
        if (response && !response.agentGroupMembers.length) {
          delete response.agentGroupMembers;
        }
        this.agentGroupForm.patchValue({ ...response });
        this.agentGroupForm.disable();
      });
  }

  getAllAgents(): void {
    this.httpService
      .get(`${AgentApiUrl.Agents}/${AgentApiUrl.getLookup}`)
      .subscribe((response) => {
        if (response) {
          this.allAgentsArr = [...response];
        }
      });
  }
}
