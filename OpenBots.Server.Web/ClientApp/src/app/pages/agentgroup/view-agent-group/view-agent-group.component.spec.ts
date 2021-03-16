import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ViewAgentGroupComponent } from './view-agent-group.component';

describe('ViewAgentGroupComponent', () => {
  let component: ViewAgentGroupComponent;
  let fixture: ComponentFixture<ViewAgentGroupComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ViewAgentGroupComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ViewAgentGroupComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
