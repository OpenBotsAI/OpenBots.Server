import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { HelperService } from '../../../@core/services/helper.service';
import { HttpService } from '../../../@core/services/http.service';
import { AgentApiUrl, AgentGroupAPiUrl } from '../../../webApiUrls';
import { AgentGroup } from '../../../interfaces/agentGroup';
@Component({
  selector: 'ngx-agent-group',
  templateUrl: './agent-group.component.html',
  styleUrls: ['./agent-group.component.scss'],
})
export class AgentGroupComponent implements OnInit {
  agentGroupForm: FormGroup;
  isSubmitted = false;
  urlId: string;
  eTag: string;
  title = 'Add';
  allAgentsArr: AgentGroup[] = [];
  constructor(
    private fb: FormBuilder,
    private httpService: HttpService,
    private router: Router,
    private route: ActivatedRoute,
    private helperService: HelperService
  ) {}

  ngOnInit(): void {
    this.urlId = this.route.snapshot.params['id'];
    this.agentGroupForm = this.initializeAgentGroupForm();
    if (this.urlId) {
      this.getAllAgents();
      this.title = 'Update';
      this.getAgentGroupById();
    }
  }

  initializeAgentGroupForm() {
    return this.fb.group({
      name: [
        '',
        [
          Validators.required,
          Validators.minLength(3),
          Validators.maxLength(100),
        ],
      ],
      isEnabled: [''],
      agentGroupMembers: [],
      description: [''],
    });
  }

  get controls() {
    return this.agentGroupForm.controls;
  }

  onSubmit() {
    this.isSubmitted = true;
    if (this.urlId) this.updateAgentGroup();
    else this.addAgentGroup();
  }

  updateAgentGroup() {
    const headers = this.helperService.getETagHeaders(this.eTag);
    const agentGroupMembers = [...this.agentGroupForm.value.agentGroupMembers];
    delete this.agentGroupForm.value.agentGroupMembers;
    this.httpService
      .put(
        `${AgentGroupAPiUrl.agentGroups}/${this.urlId}`,
        this.agentGroupForm.value,
        {
          observe: 'response',
          headers,
        }
      )
      .subscribe(
        (response) => {
          if (response && response.status === 200) {
            this.httpService
              .put(
                `${AgentGroupAPiUrl.agentGroups}/${this.urlId}/${AgentGroupAPiUrl.updateGroupMembers}`,
                agentGroupMembers
              )
              .subscribe(() => {
                this.httpService.success('Agent group updated successfully');
                this.isSubmitted = false;
                this.agentGroupForm.reset();
                this.router.navigate(['/pages/agentgroup/list']);
              });
          }
        },
        (error) => {
          if (error && error.error && error.error.status === 409) {
            this.isSubmitted = false;
            this.httpService.error(error.error.serviceErrors);
            this.getAgentGroupById();
          }
        }
      );
  }

  addAgentGroup() {
    delete this.agentGroupForm.value.agentGroupMembers;
    delete this.agentGroupForm.value.isEnabled;
    this.httpService
      .post(`${AgentGroupAPiUrl.agentGroups}`, this.agentGroupForm.value)
      .subscribe((response) => {
        if (response) {
          this.isSubmitted = false;
          this.httpService.success('Agent goup created successfully');
          this.router.navigate(['/pages/agentgroup/list']);
        }
      });
  }

  getAgentGroupById(): void {
    this.httpService
      .get(
        `${AgentGroupAPiUrl.agentGroups}/${AgentGroupAPiUrl.view}/${this.urlId}`,
        {
          observe: 'response',
        }
      )
      .subscribe((response) => {
        if (response && response.body) {
          this.eTag = response.headers.get('etag');
          this.agentGroupForm.patchValue(response.body);
        }
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
