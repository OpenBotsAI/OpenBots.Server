import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AllEmailLogComponent } from './all-email-log.component';

describe('AllEmailLogComponent', () => {
  let component: AllEmailLogComponent;
  let fixture: ComponentFixture<AllEmailLogComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AllEmailLogComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AllEmailLogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
