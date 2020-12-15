import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AllEventSubscriptionsComponent } from './all-event-subscriptions.component';

describe('AllEventSubscriptionsComponent', () => {
  let component: AllEventSubscriptionsComponent;
  let fixture: ComponentFixture<AllEventSubscriptionsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AllEventSubscriptionsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AllEventSubscriptionsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
