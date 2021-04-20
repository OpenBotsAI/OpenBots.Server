import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AllBusinessEventComponent } from './all-business-event.component';

describe('AllBusinessEventComponent', () => {
  let component: AllBusinessEventComponent;
  let fixture: ComponentFixture<AllBusinessEventComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AllBusinessEventComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AllBusinessEventComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
