import {
  Component, ElementRef, OnChanges, OnDestroy, ViewChild,
  input, afterNextRender, inject,
} from '@angular/core';
import * as d3 from 'd3';

export interface BarDatum {
  label: string;
  value: number;
  color?: string;
}

@Component({
  selector: 'app-d3-bar-chart',
  template: `<div #container class="chart-container"><svg #svg></svg></div>`,
  styles: [`
    .chart-container { width: 100%; }
    svg { width: 100%; display: block; }
  `],
})
export class D3BarChartComponent implements OnChanges, OnDestroy {
  data        = input.required<BarDatum[]>();
  height      = input<number>(110);
  barColor    = input<string>('#4854D3');
  highlightLast = input<boolean>(false);

  @ViewChild('container') containerRef!: ElementRef<HTMLDivElement>;
  @ViewChild('svg')       svgRef!: ElementRef<SVGSVGElement>;

  private resizeObserver?: ResizeObserver;
  private rendered = false;

  constructor() {
    afterNextRender(() => {
      this.setupResizeObserver();
      this.render();
      this.rendered = true;
    });
  }

  ngOnChanges() {
    if (this.rendered) this.render();
  }

  ngOnDestroy() {
    this.resizeObserver?.disconnect();
  }

  private setupResizeObserver() {
    this.resizeObserver = new ResizeObserver(() => this.render());
    this.resizeObserver.observe(this.containerRef.nativeElement);
  }

  render() {
    const container = this.containerRef?.nativeElement;
    const svgEl = this.svgRef?.nativeElement;
    if (!container || !svgEl) return;

    const width  = container.clientWidth || 300;
    const h      = this.height();
    const margin = { top: 8, right: 4, bottom: 20, left: 4 };
    const innerW = width - margin.left - margin.right;
    const innerH = h - margin.top - margin.bottom;
    const items  = this.data();
    if (!items?.length) return;

    d3.select(svgEl).selectAll('*').remove();

    const svg = d3.select(svgEl)
      .attr('viewBox', `0 0 ${width} ${h}`)
      .attr('height', h);

    const g = svg.append('g')
      .attr('transform', `translate(${margin.left},${margin.top})`);

    const x = d3.scaleBand()
      .domain(items.map(d => d.label))
      .range([0, innerW])
      .padding(0.25);

    const y = d3.scaleLinear()
      .domain([0, d3.max(items, d => d.value) ?? 1])
      .nice()
      .range([innerH, 0]);

    const defaultColor = this.barColor();
    const highlight = this.highlightLast();

    g.selectAll('rect')
      .data(items)
      .join('rect')
      .attr('x', d => x(d.label)!)
      .attr('y', d => y(d.value))
      .attr('width', x.bandwidth())
      .attr('height', d => innerH - y(d.value))
      .attr('rx', 2)
      .attr('fill', (d, i) => {
        if (highlight && i === items.length - 1) return '#4854D3';
        return d.color ?? defaultColor;
      })
      .attr('opacity', (_, i) => highlight && i === items.length - 1 ? 1 : 0.65);

    g.append('g')
      .attr('transform', `translate(0,${innerH})`)
      .call(d3.axisBottom(x).tickSize(0))
      .call(ax => ax.select('.domain').remove())
      .selectAll('text')
      .style('font-size', '10px')
      .style('fill', 'var(--color-text-tertiary)')
      .attr('dy', '1em');
  }
}
