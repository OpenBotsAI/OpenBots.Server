import {
  Directive,
  ElementRef,
  HostListener,
  Input,
  Renderer2,
} from '@angular/core';

@Directive({
  selector: '[ngxTrim]',
})
export class TrimDirective {
  private get _type(): string {
    return this.elementRef.nativeElement.type || 'text';
  }
  @Input() trim: string;
  private _value: string;

  @HostListener('blur', ['$event.type', '$event.target.value'])
  onBlur(event: string, value: string): void {
    this.updateValue(event, value.trim());
    this.onTouched();
  }

  @HostListener('input', ['$event.type', '$event.target.value'])
  onInput(event: string, value: string): void {
    this.updateValue(event, value);
  }

  onChange(_: any) {}

  onTouched() {}
  constructor(private renderer: Renderer2, private elementRef: ElementRef) {}

  registerOnChange(fn: (_: any) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  public writeValue(value: any): void {
    this._value = value === '' ? '' : value || null;

    this.renderer.setProperty(
      this.elementRef.nativeElement,
      'value',
      this._value
    );
    if (this._type !== 'text') {
      this.renderer.setAttribute(
        this.elementRef.nativeElement,
        'value',
        this._value
      );
    }
  }

  // setDisabledState(isDisabled: boolean): void {
  //   this.renderer.setProperty(
  //     this.elementRef.nativeElement,
  //     'disabled',
  //     isDisabled
  //   );
  // }

  private setCursorPointer(cursorPosition: any, hasTypedSymbol: boolean): void {
    if (
      hasTypedSymbol &&
      ['text', 'search', 'url', 'tel', 'password'].indexOf(this._type) >= 0
    ) {
      this.elementRef.nativeElement.setSelectionRange(
        cursorPosition,
        cursorPosition
      );
    }
  }

  private updateValue(event: string, value: string): void {
    value = this.trim !== '' && event !== this.trim ? value : value.trim();
    const previous = this._value;
    const cursorPosition = this.elementRef.nativeElement.selectionStart;
    this.writeValue(value);
    if ((this._value || previous) && this._value.trim() !== previous) {
      this.onChange(this._value);
    }
    const hasTypedSymbol = value && previous && value !== previous;
    this.setCursorPointer(cursorPosition, hasTypedSymbol);
  }
}
