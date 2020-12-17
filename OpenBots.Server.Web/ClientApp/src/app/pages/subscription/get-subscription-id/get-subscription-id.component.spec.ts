import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { GetSubscriptionIdComponent } from './get-subscription-id.component';

describe('GetSubscriptionIdComponent', () => {
  let component: GetSubscriptionIdComponent;
  let fixture: ComponentFixture<GetSubscriptionIdComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ GetSubscriptionIdComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(GetSubscriptionIdComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
