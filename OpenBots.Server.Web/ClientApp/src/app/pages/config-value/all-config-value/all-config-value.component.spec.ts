import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AllConfigValueComponent } from './all-config-value.component';

describe('AllConfigValueComponent', () => {
  let component: AllConfigValueComponent;
  let fixture: ComponentFixture<AllConfigValueComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AllConfigValueComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AllConfigValueComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
